using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.FlightMovements.UpdateFlightMovement;

[Authorize]
public record UpdateFlightMovementCommand(
    int Id,
    int? TerminalId,
    int? GateId,
    DateTime? BoardingOn,
    DateTime? GateCloseOn,
    DateTime ScheduledOn,
    DateTime? EstimatedOn,
    DateTime? ActualOn) : IRequest;

internal sealed class UpdateFlightMovementCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateFlightMovementCommand>
{
    public async Task Handle(UpdateFlightMovementCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.FlightMovements
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(FlightMovement), request.Id);

        entity.TerminalId = request.TerminalId;
        entity.GateId = request.GateId;
        entity.BoardingOn = request.BoardingOn;
        entity.GateCloseOn = request.GateCloseOn;
        entity.ScheduledOn = request.ScheduledOn;
        entity.EstimatedOn = request.EstimatedOn;
        entity.ActualOn = request.ActualOn;

        await context.SaveChangesAsync(cancellationToken);
    }
}
