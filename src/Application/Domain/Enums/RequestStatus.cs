using System.Text.Json.Serialization;

namespace Application.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RequestStatus
{
    Requested,
    Confirmed,
    Cancelled,
    Resolved,
}
