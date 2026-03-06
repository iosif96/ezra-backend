using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.TouchpointScans.DeleteTouchpointScan;

[Authorize]
public record DeleteTouchpointScanCommand(int Id) : IRequest;

internal sealed class DeleteTouchpointScanCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteTouchpointScanCommand>
{
    public async Task Handle(DeleteTouchpointScanCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.TouchpointScans
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(TouchpointScan), request.Id);

        context.TouchpointScans.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
