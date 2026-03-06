using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.FlightMovements.GetFlightMovementsWithPagination;

public record FlightMovementBriefResponse(int Id, int FlightId, MovementType Type, int AirportId, DateTime ScheduledOn, DateTime? ActualOn);

[Authorize]
public record GetFlightMovementsWithPaginationQuery(int? FlightId = null, MovementType? Type = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<FlightMovementBriefResponse>>;

internal sealed class GetFlightMovementsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetFlightMovementsWithPaginationQuery, PaginatedList<FlightMovementBriefResponse>>
{
    public Task<PaginatedList<FlightMovementBriefResponse>> Handle(GetFlightMovementsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.FlightMovements
            .Where(x => request.FlightId == null || x.FlightId == request.FlightId)
            .Where(x => request.Type == null || x.Type == request.Type)
            .OrderBy(x => x.ScheduledOn)
            .Select(x => new FlightMovementBriefResponse(x.Id, x.FlightId, x.Type, x.AirportId, x.ScheduledOn, x.ActualOn))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
