using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Terminals.GetTerminalsWithPagination;

public record AirportDto(int Id, string Name, string IataCode, string IcaoCode);

public record TerminalBriefResponse(int Id, AirportDto Airport, string Code);

[Authorize]
public record GetTerminalsWithPaginationQuery(int? AirportId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<TerminalBriefResponse>>;

internal sealed class GetTerminalsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTerminalsWithPaginationQuery, PaginatedList<TerminalBriefResponse>>
{
    public Task<PaginatedList<TerminalBriefResponse>> Handle(GetTerminalsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Terminals
            .Include(x => x.Airport)
            .Where(x => request.AirportId == null || x.AirportId == request.AirportId)
            .OrderBy(x => x.Code)
            .Select(x => new TerminalBriefResponse(x.Id, new AirportDto(x.Airport.Id, x.Airport.Name, x.Airport.IataCode, x.Airport.IcaoCode), x.Code))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
