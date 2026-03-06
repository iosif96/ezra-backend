using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Airports.GetAirport;

public record AirportResponse(int Id, string Name, string IataCode, string IcaoCode, string City, string CountryCode, string DefaultLanguage, string PromptInformation);

[Authorize]
public record GetAirportQuery(int Id) : IRequest<AirportResponse>;

internal sealed class GetAirportQueryHandler(ApplicationDbContext context) : IRequestHandler<GetAirportQuery, AirportResponse>
{
    public async Task<AirportResponse> Handle(GetAirportQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Airports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), request.Id);

        return new AirportResponse(entity.Id, entity.Name, entity.IataCode, entity.IcaoCode, entity.City, entity.CountryCode, entity.DefaultLanguage, entity.PromptInformation);
    }
}
