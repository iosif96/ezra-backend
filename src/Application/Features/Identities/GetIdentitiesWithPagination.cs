using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Identities.GetIdentitiesWithPagination;

public record IdentityBriefResponse(int Id, string? PassengerName);

[Authorize]
public record GetIdentitiesWithPaginationQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<IdentityBriefResponse>>;

internal sealed class GetIdentitiesWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetIdentitiesWithPaginationQuery, PaginatedList<IdentityBriefResponse>>
{
    public Task<PaginatedList<IdentityBriefResponse>> Handle(GetIdentitiesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Identities
            .OrderBy(x => x.PassengerName)
            .Select(x => new IdentityBriefResponse(x.Id, x.PassengerName))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
