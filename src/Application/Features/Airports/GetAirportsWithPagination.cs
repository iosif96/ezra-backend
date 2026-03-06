using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Airports.GetAirportsWithPagination;

public record AirportBriefResponse(int Id, string Name, string IataCode, string IcaoCode, string City, string CountryCode);

[Authorize]
public record GetAirportsWithPaginationQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<AirportBriefResponse>>;

internal sealed class GetAirportsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetAirportsWithPaginationQuery, PaginatedList<AirportBriefResponse>>
{
    public Task<PaginatedList<AirportBriefResponse>> Handle(GetAirportsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Airports
            .OrderBy(x => x.Name)
            .Select(x => new AirportBriefResponse(x.Id, x.Name, x.IataCode, x.IcaoCode, x.City, x.CountryCode))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
