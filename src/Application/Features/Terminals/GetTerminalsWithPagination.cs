using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Terminals.GetTerminalsWithPagination;

public record TerminalBriefResponse(int Id, int AirportId, string Code);

[Authorize]
public record GetTerminalsWithPaginationQuery(int? AirportId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<TerminalBriefResponse>>;

internal sealed class GetTerminalsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTerminalsWithPaginationQuery, PaginatedList<TerminalBriefResponse>>
{
    public Task<PaginatedList<TerminalBriefResponse>> Handle(GetTerminalsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Terminals
            .Where(x => request.AirportId == null || x.AirportId == request.AirportId)
            .OrderBy(x => x.Code)
            .Select(x => new TerminalBriefResponse(x.Id, x.AirportId, x.Code))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
