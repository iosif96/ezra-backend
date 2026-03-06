using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.Requests.GetRequestsWithPagination;

public record RequestBriefResponse(int Id, int ConversationId, RequestType Type, int? StaffId, RequestStatus Status);

[Authorize]
public record GetRequestsWithPaginationQuery(int? ConversationId = null, RequestStatus? Status = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<RequestBriefResponse>>;

internal sealed class GetRequestsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetRequestsWithPaginationQuery, PaginatedList<RequestBriefResponse>>
{
    public Task<PaginatedList<RequestBriefResponse>> Handle(GetRequestsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Requests
            .Where(x => request.ConversationId == null || x.ConversationId == request.ConversationId)
            .Where(x => request.Status == null || x.Status == request.Status)
            .OrderByDescending(x => x.Id)
            .Select(x => new RequestBriefResponse(x.Id, x.ConversationId, x.Type, x.StaffId, x.Status))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
