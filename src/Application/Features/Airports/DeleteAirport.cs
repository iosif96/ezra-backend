using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Airports.DeleteAirport;

[Authorize]
public record DeleteAirportCommand(int Id) : IRequest;

internal sealed class DeleteAirportCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteAirportCommand>
{
    public async Task Handle(DeleteAirportCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Airports
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Airport), request.Id);

        context.Airports.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
