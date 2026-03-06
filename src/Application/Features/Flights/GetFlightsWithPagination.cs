using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.Flights.GetFlightsWithPagination;

public record FlightBriefResponse(int Id, DateOnly Date, string Number, string IataCode, string Airline, FlightStatus Status);

[Authorize]
public record GetFlightsWithPaginationQuery(DateOnly? Date = null, FlightStatus? Status = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<FlightBriefResponse>>;

internal sealed class GetFlightsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetFlightsWithPaginationQuery, PaginatedList<FlightBriefResponse>>
{
    public Task<PaginatedList<FlightBriefResponse>> Handle(GetFlightsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Flights
            .Where(x => request.Date == null || x.Date == request.Date)
            .Where(x => request.Status == null || x.Status == request.Status)
            .OrderBy(x => x.Date)
            .Select(x => new FlightBriefResponse(x.Id, x.Date, x.Number, x.IataCode, x.Airline, x.Status))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
