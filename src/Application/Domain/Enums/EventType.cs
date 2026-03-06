using System.Text.Json.Serialization;

namespace Application.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventType
{
    BoardingOpen,
    GateChanged,
    Delay,
    Cancellation,
    Diversion,
    Custom,
}
