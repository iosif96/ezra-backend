using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.FlightMovements.DeleteFlightMovement;

[Authorize]
public record DeleteFlightMovementCommand(int Id) : IRequest;

internal sealed class DeleteFlightMovementCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteFlightMovementCommand>
{
    public async Task Handle(DeleteFlightMovementCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.FlightMovements
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(FlightMovement), request.Id);

        context.FlightMovements.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
