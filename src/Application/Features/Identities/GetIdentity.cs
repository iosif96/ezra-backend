using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Identities.GetIdentity;

public record IdentityResponse(int Id, string? PassengerName);

[Authorize]
public record GetIdentityQuery(int Id) : IRequest<IdentityResponse>;

internal sealed class GetIdentityQueryHandler(ApplicationDbContext context) : IRequestHandler<GetIdentityQuery, IdentityResponse>
{
    public async Task<IdentityResponse> Handle(GetIdentityQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Identities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Identity), request.Id);

        return new IdentityResponse(entity.Id, entity.PassengerName);
    }
}
