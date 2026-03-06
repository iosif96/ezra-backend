using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Staff.GetStaffWithPagination;

public record StaffBriefResponse(int Id, int AirportId, string Name, string PhoneNumber);

[Authorize]
public record GetStaffWithPaginationQuery(int? AirportId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<StaffBriefResponse>>;

internal sealed class GetStaffWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetStaffWithPaginationQuery, PaginatedList<StaffBriefResponse>>
{
    public Task<PaginatedList<StaffBriefResponse>> Handle(GetStaffWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Staff
            .Where(x => request.AirportId == null || x.AirportId == request.AirportId)
            .OrderBy(x => x.Name)
            .Select(x => new StaffBriefResponse(x.Id, x.AirportId, x.Name, x.PhoneNumber))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
