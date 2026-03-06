using System.Text.Json.Serialization;

namespace Application.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaType
{
    Text,
    Voice,
    Image,
    Document,
}
