using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Identities.CreateIdentity;

[Authorize]
public record CreateIdentityCommand(string? PassengerName) : IRequest<int>;

internal sealed class CreateIdentityCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateIdentityCommand, int>
{
    public async Task<int> Handle(CreateIdentityCommand request, CancellationToken cancellationToken)
    {
        var entity = new Identity
        {
            PassengerName = request.PassengerName,
        };

        context.Identities.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
