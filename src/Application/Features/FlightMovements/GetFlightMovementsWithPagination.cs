using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.FlightMovements.GetFlightMovementsWithPagination;

public record FlightBriefDto(int Id, string Number, string IataCode, string Airline, FlightStatus Status);

public record AirportBriefDto(string IataCode, string Name, string City);

public record FlightMovementBriefResponse(
    int Id,
    FlightBriefDto Flight,
    AirportBriefDto Airport,
    MovementType Type,
    string? Terminal,
    string? Gate,
    DateTime? BoardingOn,
    DateTime? GateCloseOn,
    DateTime ScheduledOn,
    DateTime? EstimatedOn,
    DateTime? ActualOn);

[Authorize]
public record GetFlightMovementsWithPaginationQuery(int? FlightId = null, MovementType? Type = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<FlightMovementBriefResponse>>;

internal sealed class GetFlightMovementsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetFlightMovementsWithPaginationQuery, PaginatedList<FlightMovementBriefResponse>>
{
    public Task<PaginatedList<FlightMovementBriefResponse>> Handle(GetFlightMovementsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.FlightMovements
            .Include(x => x.Flight)
            .Include(x => x.Airport)
            .Include(x => x.Terminal)
            .Include(x => x.Gate)
            .Where(x => request.FlightId == null || x.FlightId == request.FlightId)
            .Where(x => request.Type == null || x.Type == request.Type)
            .OrderByDescending(x => x.ScheduledOn)
            .Select(x => new FlightMovementBriefResponse(
                x.Id,
                new FlightBriefDto(x.Flight.Id, x.Flight.Number, x.Flight.IataCode, x.Flight.Airline, x.Flight.Status),
                new AirportBriefDto(x.Airport.IataCode, x.Airport.Name, x.Airport.City),
                x.Type,
                x.Terminal != null ? x.Terminal.Code : null,
                x.Gate != null ? x.Gate.Code : null,
                x.BoardingOn,
                x.GateCloseOn,
                x.ScheduledOn,
                x.EstimatedOn,
                x.ActualOn))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
