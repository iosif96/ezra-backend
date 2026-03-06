using System.Text.Json.Serialization;

namespace Application.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlightSource
{
    AviationStack,
    FlightAware,
    OpenSky,
    Manual,
}
