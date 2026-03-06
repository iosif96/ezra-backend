using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Identities.UpdateIdentity;

[Authorize]
public record UpdateIdentityCommand(int Id, string? PassengerName) : IRequest;

internal sealed class UpdateIdentityCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateIdentityCommand>
{
    public async Task Handle(UpdateIdentityCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Identities
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Identity), request.Id);

        entity.PassengerName = request.PassengerName;

        await context.SaveChangesAsync(cancellationToken);
    }
}
