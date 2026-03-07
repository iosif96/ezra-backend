using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Merchants.GetMerchantsWithPagination;

public record AirportDto(int Id, string Name, string IataCode, string IcaoCode);

public record TerminalDto(int Id, AirportDto Airport, string Code);

public record MerchantBriefResponse(int Id, TerminalDto Terminal, string Name, bool IsAirside);

[Authorize]
public record GetMerchantsWithPaginationQuery(int? TerminalId = null, bool? IsAirside = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<MerchantBriefResponse>>;

internal sealed class GetMerchantsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetMerchantsWithPaginationQuery, PaginatedList<MerchantBriefResponse>>
{
    public Task<PaginatedList<MerchantBriefResponse>> Handle(GetMerchantsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Merchants
            .Include(x => x.Terminal)
                .ThenInclude(x => x.Airport)
            .Where(x => request.TerminalId == null || x.TerminalId == request.TerminalId)
            .Where(x => request.IsAirside == null || x.IsAirside == request.IsAirside)
            .OrderBy(x => x.Name)
            .Select(x => new MerchantBriefResponse(x.Id, new TerminalDto(x.Terminal.Id, new AirportDto(x.Terminal.Airport.Id, x.Terminal.Airport.Name, x.Terminal.Airport.IataCode, x.Terminal.Airport.IcaoCode), x.Terminal.Code), x.Name, x.IsAirside))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
