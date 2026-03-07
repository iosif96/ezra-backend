using System.Text.Json.Serialization;

namespace Application.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChannelType
{
    Whatsapp,
    Messenger,
    Telegram,
    Web,
}
