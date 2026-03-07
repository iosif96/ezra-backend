using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Conversations.GetConversationsWithPagination;

public record IdentityDto(int Id, string? PassengerName);

public record ConversationBriefResponse(int Id, ChannelType ChannelType, string ChannelId, IdentityDto? Identity, DateTime LastMessageOn, DateTime Created);

[Authorize]
public record GetConversationsWithPaginationQuery(ChannelType? ChannelType = null, int? IdentityId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<ConversationBriefResponse>>;

internal sealed class GetConversationsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetConversationsWithPaginationQuery, PaginatedList<ConversationBriefResponse>>
{
    public Task<PaginatedList<ConversationBriefResponse>> Handle(GetConversationsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Conversations
            .Include(x => x.Identity)
            .Where(x => request.ChannelType == null || x.ChannelType == request.ChannelType)
            .Where(x => request.IdentityId == null || x.IdentityId == request.IdentityId)
            .OrderByDescending(x => x.LastMessageOn)
            .Select(x => new ConversationBriefResponse(
                x.Id,
                x.ChannelType,
                x.ChannelId,
                x.Identity != null ? new IdentityDto(x.Identity.Id, x.Identity.PassengerName) : null,
                x.LastMessageOn,
                x.Created))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
