using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Gates.GetGatesWithPagination;

public record GateBriefResponse(int Id, int TerminalId, string Code);

[Authorize]
public record GetGatesWithPaginationQuery(int? TerminalId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<GateBriefResponse>>;

internal sealed class GetGatesWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetGatesWithPaginationQuery, PaginatedList<GateBriefResponse>>
{
    public Task<PaginatedList<GateBriefResponse>> Handle(GetGatesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Gates
            .Where(x => request.TerminalId == null || x.TerminalId == request.TerminalId)
            .OrderBy(x => x.Code)
            .Select(x => new GateBriefResponse(x.Id, x.TerminalId, x.Code))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
