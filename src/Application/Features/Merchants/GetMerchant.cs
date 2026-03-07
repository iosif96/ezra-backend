using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Merchants.GetMerchant;

public record AirportDto(int Id, string Name, string IataCode, string IcaoCode);

public record TerminalDto(int Id, AirportDto Airport, string Code);

public record MerchantResponse(int Id, TerminalDto Terminal, string Name, bool IsAirside, float Latitude, float Longitude, string PromptInformation);

[Authorize]
public record GetMerchantQuery(int Id) : IRequest<MerchantResponse>;

internal sealed class GetMerchantQueryHandler(ApplicationDbContext context) : IRequestHandler<GetMerchantQuery, MerchantResponse>
{
    public async Task<MerchantResponse> Handle(GetMerchantQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Merchants
            .Include(x => x.Terminal)
                .ThenInclude(x => x.Airport)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Merchant), request.Id);

        var airportDto = new AirportDto(entity.Terminal.Airport.Id, entity.Terminal.Airport.Name, entity.Terminal.Airport.IataCode, entity.Terminal.Airport.IcaoCode);
        var terminalDto = new TerminalDto(entity.Terminal.Id, airportDto, entity.Terminal.Code);
        return new MerchantResponse(entity.Id, terminalDto, entity.Name, entity.IsAirside, entity.Latitude, entity.Longitude, entity.PromptInformation);
    }
}
