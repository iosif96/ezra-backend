using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Flights.DeleteFlight;

[Authorize]
public record DeleteFlightCommand(int Id) : IRequest;

internal sealed class DeleteFlightCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteFlightCommand>
{
    public async Task Handle(DeleteFlightCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Flights
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Flight), request.Id);

        context.Flights.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
