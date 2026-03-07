using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messages.GetMessagesWithPagination;

public record IdentityDto(int Id, string? PassengerName);

public record ConversationDto(int Id, ChannelType ChannelType, string ChannelId, IdentityDto? Identity);

public record MessageBriefResponse(int Id, ConversationDto Conversation, MessageType Type, MediaType MediaType, string? Content, DateTime Created);

[Authorize]
public record GetMessagesWithPaginationQuery(int? ConversationId = null, MessageType? Type = null, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<MessageBriefResponse>>;

internal sealed class GetMessagesWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetMessagesWithPaginationQuery, PaginatedList<MessageBriefResponse>>
{
    public Task<PaginatedList<MessageBriefResponse>> Handle(GetMessagesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Messages
            .Include(x => x.Conversation).ThenInclude(x => x.Identity)
            .Where(x => request.ConversationId == null || x.ConversationId == request.ConversationId)
            .Where(x => request.Type == null || x.Type == request.Type)
            .OrderBy(x => x.Id)
            .Select(x => new MessageBriefResponse(
                x.Id,
                new ConversationDto(
                    x.Conversation.Id,
                    x.Conversation.ChannelType,
                    x.Conversation.ChannelId,
                    x.Conversation.Identity != null ? new IdentityDto(x.Conversation.Identity.Id, x.Conversation.Identity.PassengerName) : null),
                x.Type,
                x.MediaType,
                x.Content,
                x.Created))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
