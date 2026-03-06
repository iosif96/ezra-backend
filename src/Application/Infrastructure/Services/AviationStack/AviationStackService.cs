using Application.Common.Configuration;
using Application.Common.Interfaces;
using Application.Common.Models.AviationStack;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Application.Infrastructure.Services.AviationStack;

public class AviationStackService : IAviationStackService
{
    private readonly HttpClient _httpClient;
    private readonly AviationStackConfiguration _config;
    private readonly ILogger<AviationStackService> _logger;

    public AviationStackService(
        HttpClient httpClient,
        IOptions<AviationStackConfiguration> config,
        ILogger<AviationStackService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<List<AviationStackFlight>> GetFlightsAsync(string airportIata, DateOnly date, CancellationToken cancellationToken = default)
    {
        var departures = await FetchAllPagesAsync("dep_iata", airportIata, date, cancellationToken);
        var arrivals = await FetchAllPagesAsync("arr_iata", airportIata, date, cancellationToken);

        return departures.Concat(arrivals).ToList();
    }

    private async Task<List<AviationStackFlight>> FetchAllPagesAsync(string directionParam, string airportIata, DateOnly date, CancellationToken cancellationToken)
    {
        var allFlights = new List<AviationStackFlight>();
        var offset = 0;
        const int limit = 100;

        while (true)
        {
            var url = $"{_config.BaseUrl}/flights" +
                $"?access_key={_config.ApiKey}" +
                $"&{directionParam}={airportIata}" +
                $"&flight_date={date:yyyy-MM-dd}" +
                $"&limit={limit}" +
                $"&offset={offset}";

            _logger.LogInformation("AviationStack request: {Direction}={Airport} date={Date} offset={Offset}", directionParam, airportIata, date, offset);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonConvert.DeserializeObject<AviationStackResponse>(json);

            if (result?.Data == null || result.Data.Count == 0)
                break;

            allFlights.AddRange(result.Data);

            if (allFlights.Count >= result.Pagination.Total)
                break;

            offset += limit;
        }

        _logger.LogInformation("AviationStack fetched {Count} flights for {Direction}={Airport} on {Date}", allFlights.Count, directionParam, airportIata, date);

        return allFlights;
    }
}
