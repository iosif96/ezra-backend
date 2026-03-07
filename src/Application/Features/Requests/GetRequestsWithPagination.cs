using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Requests.GetRequestsWithPagination;

public record IdentityDto(int Id, string? PassengerName);

public record ConversationDto(int Id, ChannelType ChannelType, string ChannelId, IdentityDto? Identity);

public record StaffDto(int Id, string Name);

public record RequestBriefResponse(int Id, ConversationDto Conversation, RequestType Type, string? Content, StaffDto? Staff, RequestStatus Status, DateTime Created);

[Authorize]
public record GetRequestsWithPaginationQuery(int? ConversationId = null, RequestStatus? Status = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<RequestBriefResponse>>;

internal sealed class GetRequestsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetRequestsWithPaginationQuery, PaginatedList<RequestBriefResponse>>
{
    public Task<PaginatedList<RequestBriefResponse>> Handle(GetRequestsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Requests
            .Include(x => x.Conversation).ThenInclude(x => x.Identity)
            .Include(x => x.Staff)
            .Where(x => request.ConversationId == null || x.ConversationId == request.ConversationId)
            .Where(x => request.Status == null || x.Status == request.Status)
            .OrderByDescending(x => x.Created)
            .Select(x => new RequestBriefResponse(
                x.Id,
                new ConversationDto(
                    x.Conversation.Id,
                    x.Conversation.ChannelType,
                    x.Conversation.ChannelId,
                    x.Conversation.Identity != null ? new IdentityDto(x.Conversation.Identity.Id, x.Conversation.Identity.PassengerName) : null),
                x.Type,
                x.Content,
                x.Staff != null ? new StaffDto(x.Staff.Id, x.Staff.Name) : null,
                x.Status,
                x.Created))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
