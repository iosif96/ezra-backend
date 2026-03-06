using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Touchpoints.DeleteTouchpoint;

[Authorize]
public record DeleteTouchpointCommand(int Id) : IRequest;

internal sealed class DeleteTouchpointCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteTouchpointCommand>
{
    public async Task Handle(DeleteTouchpointCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Touchpoints
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Touchpoint), request.Id);

        context.Touchpoints.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
