using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Features.Conversations.ProcessIncomingMessage;
using Application.Infrastructure.Persistence;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messages.CreateMessage;

public record CreateMessageResponse(
    int MessageId,
    List<AssistantMessageDto>? AssistantMessages);

public record AssistantMessageDto(
    int Id,
    string? Content,
    DateTime Created);

public record CreateMessageCommand(
    int ConversationId,
    MessageType Type,
    MediaType MediaType,
    string? MediaUrl,
    string? Content,
    string? Model,
    int? InputTokens,
    int? OutputTokens,
    float? StressIndex) : IRequest<CreateMessageResponse>;

public class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).GreaterThan(0);
        RuleFor(x => x.StressIndex).InclusiveBetween(0f, 1f).When(x => x.StressIndex.HasValue);
    }
}

internal sealed class CreateMessageCommandHandler(ApplicationDbContext context, ISender sender) : IRequestHandler<CreateMessageCommand, CreateMessageResponse>
{
    public async Task<CreateMessageResponse> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var entity = new Message
        {
            ConversationId = request.ConversationId,
            Type = request.Type,
            MediaType = request.MediaType,
            MediaUrl = request.MediaUrl,
            Content = request.Content,
            Model = request.Model,
            InputTokens = request.InputTokens,
            OutputTokens = request.OutputTokens,
            StressIndex = request.StressIndex,
        };

        context.Messages.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        List<AssistantMessageDto>? assistantMessages = null;

        if (request.Type == MessageType.User)
        {
            var conversation = await context.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

            await sender.Send(new ProcessIncomingMessageCommand(
                conversation.ChannelType,
                conversation.ChannelId,
                request.Content,
                null, null, null), cancellationToken);

            assistantMessages = await context.Messages
                .AsNoTracking()
                .Where(m => m.ConversationId == request.ConversationId
                    && m.Type == MessageType.Assistant
                    && m.Id > entity.Id)
                .OrderBy(m => m.Id)
                .Select(m => new AssistantMessageDto(m.Id, m.Content, m.Created))
                .ToListAsync(cancellationToken);
        }

        return new CreateMessageResponse(entity.Id, assistantMessages);
    }
}
