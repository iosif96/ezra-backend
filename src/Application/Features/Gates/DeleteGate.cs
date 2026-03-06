using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Gates.DeleteGate;

[Authorize]
public record DeleteGateCommand(int Id) : IRequest;

internal sealed class DeleteGateCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteGateCommand>
{
    public async Task Handle(DeleteGateCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Gates
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Gate), request.Id);

        context.Gates.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
