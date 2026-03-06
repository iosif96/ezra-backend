using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.FlightMovements.GetFlightMovement;

public record FlightMovementResponse(
    int Id, int FlightId, MovementType Type, int AirportId,
    int? TerminalId, int? GateId,
    DateTime? BoardingOn, DateTime? GateCloseOn,
    DateTime ScheduledOn, DateTime? EstimatedOn, DateTime? ActualOn);

[Authorize]
public record GetFlightMovementQuery(int Id) : IRequest<FlightMovementResponse>;

internal sealed class GetFlightMovementQueryHandler(ApplicationDbContext context) : IRequestHandler<GetFlightMovementQuery, FlightMovementResponse>
{
    public async Task<FlightMovementResponse> Handle(GetFlightMovementQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.FlightMovements
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FlightMovement), request.Id);

        return new FlightMovementResponse(entity.Id, entity.FlightId, entity.Type, entity.AirportId,
            entity.TerminalId, entity.GateId, entity.BoardingOn, entity.GateCloseOn,
            entity.ScheduledOn, entity.EstimatedOn, entity.ActualOn);
    }
}
