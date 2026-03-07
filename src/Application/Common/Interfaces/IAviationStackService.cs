using Application.Common.Models.AviationStack;
using Application.Domain.Enums;

namespace Application.Common.Interfaces;

public interface IAviationStackService
{
    /// <summary>
    /// Fetches all current flights for a given airport and direction from AviationStack.
    /// Free tier does not support date filtering — returns whatever the API has.
    /// </summary>
    Task<List<AviationStackFlight>> GetFlightsAsync(string airportIata, MovementType direction, CancellationToken cancellationToken = default);
}
