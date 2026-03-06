using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Requests.DeleteRequest;

[Authorize]
public record DeleteRequestCommand(int Id) : IRequest;

internal sealed class DeleteRequestCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteRequestCommand>
{
    public async Task Handle(DeleteRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Requests
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Application.Domain.Entities.Request), request.Id);

        context.Requests.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
