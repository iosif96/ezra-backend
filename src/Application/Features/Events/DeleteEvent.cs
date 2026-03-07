using Application.Common.Security;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Events.DeleteEvent;

[Authorize]
public record DeleteEventCommand(int Id) : IRequest;

internal sealed class DeleteEventCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteEventCommand>
{
    public async Task Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Events
            .Include(e => e.Messages)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Application.Domain.Entities.Event), request.Id);

        if (entity.Messages.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete an event that has already been processed.");
        }

        context.Events.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
