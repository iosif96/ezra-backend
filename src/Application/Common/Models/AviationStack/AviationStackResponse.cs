using Newtonsoft.Json;

namespace Application.Common.Models.AviationStack;

public class AviationStackResponse
{
    [JsonProperty("pagination")]
    public AviationStackPagination Pagination { get; set; } = new();

    [JsonProperty("data")]
    public List<AviationStackFlight>? Data { get; set; }

    [JsonProperty("error")]
    public AviationStackError? Error { get; set; }
}

public class AviationStackError
{
    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }
}

public class AviationStackPagination
{
    [JsonProperty("limit")]
    public int Limit { get; set; }

    [JsonProperty("offset")]
    public int Offset { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }
}

public class AviationStackFlight
{
    [JsonProperty("flight_date")]
    public string? FlightDate { get; set; }

    [JsonProperty("flight_status")]
    public string? FlightStatus { get; set; }

    [JsonProperty("departure")]
    public AviationStackMovement Departure { get; set; } = new();

    [JsonProperty("arrival")]
    public AviationStackMovement Arrival { get; set; } = new();

    [JsonProperty("airline")]
    public AviationStackAirline Airline { get; set; } = new();

    [JsonProperty("flight")]
    public AviationStackFlightInfo Flight { get; set; } = new();
}

public class AviationStackMovement
{
    [JsonProperty("airport")]
    public string? Airport { get; set; }

    [JsonProperty("iata")]
    public string? Iata { get; set; }

    [JsonProperty("icao")]
    public string? Icao { get; set; }

    [JsonProperty("terminal")]
    public string? Terminal { get; set; }

    [JsonProperty("gate")]
    public string? Gate { get; set; }

    [JsonProperty("delay")]
    public int? Delay { get; set; }

    [JsonProperty("scheduled")]
    public DateTime? Scheduled { get; set; }

    [JsonProperty("estimated")]
    public DateTime? Estimated { get; set; }

    [JsonProperty("actual")]
    public DateTime? Actual { get; set; }
}

public class AviationStackAirline
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("iata")]
    public string? Iata { get; set; }

    [JsonProperty("icao")]
    public string? Icao { get; set; }
}

public class AviationStackFlightInfo
{
    [JsonProperty("number")]
    public string? Number { get; set; }

    [JsonProperty("iata")]
    public string? Iata { get; set; }

    [JsonProperty("icao")]
    public string? Icao { get; set; }
}
