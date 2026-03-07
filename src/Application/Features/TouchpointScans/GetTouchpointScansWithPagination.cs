using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.TouchpointScans.GetTouchpointScansWithPagination;

public record TouchpointScanBriefResponse(int Id, int TouchpointId, string TouchpointLabel, ChannelType ChannelType, float Latitude, float Longitude, DateTime Created);

[Authorize]
public record GetTouchpointScansWithPaginationQuery(int? TouchpointId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<TouchpointScanBriefResponse>>;

internal sealed class GetTouchpointScansWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTouchpointScansWithPaginationQuery, PaginatedList<TouchpointScanBriefResponse>>
{
    public Task<PaginatedList<TouchpointScanBriefResponse>> Handle(GetTouchpointScansWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.TouchpointScans
            .Where(x => request.TouchpointId == null || x.TouchpointId == request.TouchpointId)
            .OrderByDescending(x => x.Id)
            .Select(x => new TouchpointScanBriefResponse(x.Id, x.TouchpointId, x.Touchpoint.Label, x.ChannelType, x.Touchpoint.Latitude, x.Touchpoint.Longitude, x.Created))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
