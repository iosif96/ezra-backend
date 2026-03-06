using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Events.DeleteEvent;

[Authorize]
public record DeleteEventCommand(int Id) : IRequest;

internal sealed class DeleteEventCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteEventCommand>
{
    public async Task Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Events
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Application.Domain.Entities.Event), request.Id);

        context.Events.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
