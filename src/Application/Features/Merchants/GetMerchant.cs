using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Merchants.GetMerchant;

public record MerchantResponse(int Id, int TerminalId, string Name, bool IsAirside, float Latitude, float Longitude, string PromptInformation);

[Authorize]
public record GetMerchantQuery(int Id) : IRequest<MerchantResponse>;

internal sealed class GetMerchantQueryHandler(ApplicationDbContext context) : IRequestHandler<GetMerchantQuery, MerchantResponse>
{
    public async Task<MerchantResponse> Handle(GetMerchantQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Merchants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Merchant), request.Id);

        return new MerchantResponse(entity.Id, entity.TerminalId, entity.Name, entity.IsAirside, entity.Latitude, entity.Longitude, entity.PromptInformation);
    }
}
