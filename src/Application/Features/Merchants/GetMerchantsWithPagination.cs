using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Merchants.GetMerchantsWithPagination;

public record MerchantBriefResponse(int Id, int TerminalId, string Name, bool IsAirside);

[Authorize]
public record GetMerchantsWithPaginationQuery(int? TerminalId = null, bool? IsAirside = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<MerchantBriefResponse>>;

internal sealed class GetMerchantsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetMerchantsWithPaginationQuery, PaginatedList<MerchantBriefResponse>>
{
    public Task<PaginatedList<MerchantBriefResponse>> Handle(GetMerchantsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Merchants
            .Where(x => request.TerminalId == null || x.TerminalId == request.TerminalId)
            .Where(x => request.IsAirside == null || x.IsAirside == request.IsAirside)
            .OrderBy(x => x.Name)
            .Select(x => new MerchantBriefResponse(x.Id, x.TerminalId, x.Name, x.IsAirside))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
