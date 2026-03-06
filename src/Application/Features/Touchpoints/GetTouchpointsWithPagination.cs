using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Touchpoints.GetTouchpointsWithPagination;

public record TouchpointBriefResponse(int Id, int AirportId, int? TerminalId, int? GateId, string Label);

[Authorize]
public record GetTouchpointsWithPaginationQuery(int? AirportId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<TouchpointBriefResponse>>;

internal sealed class GetTouchpointsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTouchpointsWithPaginationQuery, PaginatedList<TouchpointBriefResponse>>
{
    public Task<PaginatedList<TouchpointBriefResponse>> Handle(GetTouchpointsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Touchpoints
            .Where(x => request.AirportId == null || x.AirportId == request.AirportId)
            .OrderBy(x => x.Label)
            .Select(x => new TouchpointBriefResponse(x.Id, x.AirportId, x.TerminalId, x.GateId, x.Label))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
