using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Gates.GetGatesWithPagination;

public record AirportDto(int Id, string Name, string IataCode, string IcaoCode);

public record TerminalDto(int Id, AirportDto Airport, string Code);

public record GateBriefResponse(int Id, TerminalDto Terminal, string Code);

[Authorize]
public record GetGatesWithPaginationQuery(int? TerminalId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<GateBriefResponse>>;

internal sealed class GetGatesWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetGatesWithPaginationQuery, PaginatedList<GateBriefResponse>>
{
    public Task<PaginatedList<GateBriefResponse>> Handle(GetGatesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Gates
            .Include(x => x.Terminal)
                .ThenInclude(x => x.Airport)
            .Where(x => request.TerminalId == null || x.TerminalId == request.TerminalId)
            .OrderBy(x => x.Code)
            .Select(x => new GateBriefResponse(x.Id, new TerminalDto(x.Terminal.Id, new AirportDto(x.Terminal.Airport.Id, x.Terminal.Airport.Name, x.Terminal.Airport.IataCode, x.Terminal.Airport.IcaoCode), x.Terminal.Code), x.Code))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
