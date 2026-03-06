using System.Text.Json.Serialization;

namespace Application.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlightStatus
{
    Scheduled,
    Estimated,
    Active,
    Landed,
    Arrived,
    Departed,
    Cancelled,
    Diverted,
    Unknown,
}
