using Application.Common.Interfaces;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Events.ProcessEvent;

public record ProcessEventCommand(int EventId) : IRequest;

internal sealed class ProcessEventHandler(
    ApplicationDbContext context,
    IMessagingChannel messagingChannel,
    ILogger<ProcessEventHandler> logger) : IRequestHandler<ProcessEventCommand>
{
    public async Task Handle(ProcessEventCommand command, CancellationToken cancellationToken)
    {
        var ev = await context.Events
            .Include(e => e.Flight)
            .FirstOrDefaultAsync(e => e.Id == command.EventId, cancellationToken);

        if (ev is null)
        {
            logger.LogWarning("Event {EventId} not found", command.EventId);
            return;
        }

        if (ev.FlightId is null)
        {
            logger.LogWarning("Event {EventId} has no flight linked", command.EventId);
            return;
        }

        // Find all conversations whose passenger has a boarding pass on this flight
        var conversations = await context.Conversations
            .Where(c => c.IdentityId != null
                && c.Identity!.BoardingPasses.Any(bp => bp.FlightId == ev.FlightId))
            .ToListAsync(cancellationToken);

        if (conversations.Count == 0)
        {
            logger.LogInformation("Event {EventId}: no conversations to notify", command.EventId);
            return;
        }

        logger.LogInformation("Event {EventId}: notifying {Count} conversations", command.EventId, conversations.Count);

        foreach (var conversation in conversations)
        {
            // Save notification message to DB
            context.Messages.Add(new Message
            {
                ConversationId = conversation.Id,
                EventId = ev.Id,
                Type = MessageType.System,
                MediaType = MediaType.Text,
                Content = ev.Content,
            });

            // Send via messaging channel
            try
            {
                await messagingChannel.SendTextMessageAsync(conversation.ChannelId, ev.Content, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Event {EventId}: failed to send to conversation {ConversationId}",
                    command.EventId, conversation.Id);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
