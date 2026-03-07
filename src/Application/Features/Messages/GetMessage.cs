using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messages.GetMessage;

public record IdentityDto(int Id, string? PassengerName);

public record ConversationDto(int Id, ChannelType ChannelType, string ChannelId, IdentityDto? Identity);

public record MessageResponse(
    int Id, ConversationDto Conversation, MessageType Type, MediaType MediaType,
    string? MediaUrl, string? Content, string? Model,
    int? InputTokens, int? OutputTokens, float? StressIndex, DateTime Created);

[Authorize]
public record GetMessageQuery(int Id) : IRequest<MessageResponse>;

internal sealed class GetMessageQueryHandler(ApplicationDbContext context) : IRequestHandler<GetMessageQuery, MessageResponse>
{
    public async Task<MessageResponse> Handle(GetMessageQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Messages
            .AsNoTracking()
            .Include(x => x.Conversation).ThenInclude(x => x.Identity)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Message), request.Id);

        return new MessageResponse(
            entity.Id,
            new ConversationDto(
                entity.Conversation.Id,
                entity.Conversation.ChannelType,
                entity.Conversation.ChannelId,
                entity.Conversation.Identity != null ? new IdentityDto(entity.Conversation.Identity.Id, entity.Conversation.Identity.PassengerName) : null),
            entity.Type,
            entity.MediaType,
            entity.MediaUrl,
            entity.Content,
            entity.Model,
            entity.InputTokens,
            entity.OutputTokens,
            entity.StressIndex,
            entity.Created);
    }
}
