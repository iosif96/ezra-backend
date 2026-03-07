using Application.Common.Configuration;
using Application.Common.Interfaces;
using Application.Common.Models.AviationStack;
using Application.Domain.Enums;

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

    public async Task<List<AviationStackFlight>> GetFlightsAsync(
        string airportIata,
        MovementType direction,
        CancellationToken cancellationToken = default)
    {
        var directionParam = direction == MovementType.Departure ? "dep_iata" : "arr_iata";
        var allFlights = new List<AviationStackFlight>();
        var offset = 0;
        const int limit = 100;

        _logger.LogInformation(
            "AviationStack sync started: {Direction} {Airport}",
            direction, airportIata);

        while (true)
        {
            var response = await FetchPageAsync(directionParam, airportIata, limit, offset, cancellationToken);

            if (response.Data == null || response.Data.Count == 0)
                break;

            allFlights.AddRange(response.Data);

            if (allFlights.Count >= response.Pagination.Total)
                break;

            offset += limit;
        }

        _logger.LogInformation(
            "AviationStack sync finished: {Count} {Direction} flights for {Airport}",
            allFlights.Count, direction, airportIata);

        return allFlights;
    }

    private async Task<AviationStackResponse> FetchPageAsync(
        string directionParam,
        string airportIata,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        // Free tier does not support flight_date — omit it
        var url = $"{_config.BaseUrl}/flights" +
            $"?access_key={_config.ApiKey}" +
            $"&{directionParam}={airportIata}" +
            $"&limit={limit}" +
            $"&offset={offset}";

        var httpResponse = await _httpClient.GetAsync(url, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError(
                "AviationStack HTTP {StatusCode} for {Direction}={Airport}",
                (int)httpResponse.StatusCode, directionParam, airportIata);

            throw new HttpRequestException(
                $"AviationStack returned HTTP {(int)httpResponse.StatusCode}");
        }

        var json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonConvert.DeserializeObject<AviationStackResponse>(json)
            ?? throw new InvalidOperationException("AviationStack returned an empty response body.");

        if (result.Error != null)
        {
            _logger.LogError(
                "AviationStack API error {Code}: {Message}",
                result.Error.Code, result.Error.Message);

            throw new InvalidOperationException(
                $"AviationStack error [{result.Error.Code}]: {result.Error.Message}");
        }

        return result;
    }
}
