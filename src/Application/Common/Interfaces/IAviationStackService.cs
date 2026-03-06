using Application.Common.Models.AviationStack;

namespace Application.Common.Interfaces;

public interface IAviationStackService
{
    Task<List<AviationStackFlight>> GetFlightsAsync(string airportIata, DateOnly date, CancellationToken cancellationToken = default);
}
