using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.GetStaffWithPagination;

public record AirportDto(int Id, string Name, string IataCode);

public record StaffBriefResponse(int Id, AirportDto Airport, string Name, string PhoneNumber);

[Authorize]
public record GetStaffWithPaginationQuery(int? AirportId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<StaffBriefResponse>>;

internal sealed class GetStaffWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetStaffWithPaginationQuery, PaginatedList<StaffBriefResponse>>
{
    public Task<PaginatedList<StaffBriefResponse>> Handle(GetStaffWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Staff
            .Include(x => x.Airport)
            .Where(x => request.AirportId == null || x.AirportId == request.AirportId)
            .OrderBy(x => x.Name)
            .Select(x => new StaffBriefResponse(
                x.Id,
                new AirportDto(x.Airport.Id, x.Airport.Name, x.Airport.IataCode),
                x.Name,
                x.PhoneNumber))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
