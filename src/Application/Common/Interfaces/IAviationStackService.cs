using Application.Common.Models.AviationStack;
using Application.Domain.Enums;

namespace Application.Common.Interfaces;

public interface IAviationStackService
{
    /// <summary>
    /// Fetches all flights for a given airport, date, and direction from AviationStack.
    /// </summary>
    Task<List<AviationStackFlight>> GetFlightsAsync(string airportIata, DateOnly date, MovementType direction, CancellationToken cancellationToken = default);
}
