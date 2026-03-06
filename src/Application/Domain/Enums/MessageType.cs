using System.Text.Json.Serialization;

namespace Application.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageType
{
    System,
    Assistant,
    User,
}
