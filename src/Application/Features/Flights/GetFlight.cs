using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Flights.GetFlight;

public record FlightResponse(int Id, DateOnly Date, string Number, string IataCode, string IcaoCode, string Airline, FlightStatus Status, FlightSource FlightSource);

[Authorize]
public record GetFlightQuery(int Id) : IRequest<FlightResponse>;

internal sealed class GetFlightQueryHandler(ApplicationDbContext context) : IRequestHandler<GetFlightQuery, FlightResponse>
{
    public async Task<FlightResponse> Handle(GetFlightQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Flights
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), request.Id);

        return new FlightResponse(entity.Id, entity.Date, entity.Number, entity.IataCode, entity.IcaoCode, entity.Airline, entity.Status, entity.FlightSource);
    }
}
