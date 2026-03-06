using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Identities.DeleteIdentity;

[Authorize]
public record DeleteIdentityCommand(int Id) : IRequest;

internal sealed class DeleteIdentityCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteIdentityCommand>
{
    public async Task Handle(DeleteIdentityCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Identities
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Identity), request.Id);

        context.Identities.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
