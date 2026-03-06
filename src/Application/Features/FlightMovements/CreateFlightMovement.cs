using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.FlightMovements.CreateFlightMovement;

[Authorize]
public record CreateFlightMovementCommand(
    int FlightId,
    MovementType Type,
    int AirportId,
    int? TerminalId,
    int? GateId,
    DateTime? BoardingOn,
    DateTime? GateCloseOn,
    DateTime ScheduledOn,
    DateTime? EstimatedOn,
    DateTime? ActualOn) : IRequest<int>;

public class CreateFlightMovementCommandValidator : AbstractValidator<CreateFlightMovementCommand>
{
    public CreateFlightMovementCommandValidator()
    {
        RuleFor(x => x.FlightId).GreaterThan(0);
        RuleFor(x => x.AirportId).GreaterThan(0);
    }
}

internal sealed class CreateFlightMovementCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateFlightMovementCommand, int>
{
    public async Task<int> Handle(CreateFlightMovementCommand request, CancellationToken cancellationToken)
    {
        var entity = new FlightMovement
        {
            FlightId = request.FlightId,
            Type = request.Type,
            AirportId = request.AirportId,
            TerminalId = request.TerminalId,
            GateId = request.GateId,
            BoardingOn = request.BoardingOn,
            GateCloseOn = request.GateCloseOn,
            ScheduledOn = request.ScheduledOn,
            EstimatedOn = request.EstimatedOn,
            ActualOn = request.ActualOn,
        };

        context.FlightMovements.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
