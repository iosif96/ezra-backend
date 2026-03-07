using Newtonsoft.Json;

namespace Application.Infrastructure.Services.WhatsApp;

// Incoming webhook payload

public class WhatsAppWebhookPayload
{
    [JsonProperty("object")]
    public string Object { get; set; } = null!;

    [JsonProperty("entry")]
    public List<WhatsAppEntry> Entry { get; set; } = [];
}

public class WhatsAppEntry
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("changes")]
    public List<WhatsAppChange> Changes { get; set; } = [];
}

public class WhatsAppChange
{
    [JsonProperty("value")]
    public WhatsAppValue Value { get; set; } = null!;

    [JsonProperty("field")]
    public string Field { get; set; } = null!;
}

public class WhatsAppValue
{
    [JsonProperty("messaging_product")]
    public string MessagingProduct { get; set; } = null!;

    [JsonProperty("metadata")]
    public WhatsAppMetadata Metadata { get; set; } = null!;

    [JsonProperty("contacts")]
    public List<WhatsAppContact>? Contacts { get; set; }

    [JsonProperty("messages")]
    public List<WhatsAppMessage>? Messages { get; set; }

    [JsonProperty("statuses")]
    public List<WhatsAppStatus>? Statuses { get; set; }
}

public class WhatsAppMetadata
{
    [JsonProperty("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = null!;

    [JsonProperty("phone_number_id")]
    public string PhoneNumberId { get; set; } = null!;
}

public class WhatsAppContact
{
    [JsonProperty("profile")]
    public WhatsAppProfile Profile { get; set; } = null!;

    [JsonProperty("wa_id")]
    public string WaId { get; set; } = null!;
}

public class WhatsAppProfile
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;
}

public class WhatsAppMessage
{
    [JsonProperty("from")]
    public string From { get; set; } = null!;

    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("timestamp")]
    public string Timestamp { get; set; } = null!;

    [JsonProperty("type")]
    public string Type { get; set; } = null!;

    [JsonProperty("text")]
    public WhatsAppText? Text { get; set; }

    [JsonProperty("image")]
    public WhatsAppMedia? Image { get; set; }

    [JsonProperty("audio")]
    public WhatsAppMedia? Audio { get; set; }

    [JsonProperty("document")]
    public WhatsAppMedia? Document { get; set; }

    [JsonProperty("location")]
    public WhatsAppLocation? Location { get; set; }
}

public class WhatsAppText
{
    [JsonProperty("body")]
    public string Body { get; set; } = null!;
}

public class WhatsAppMedia
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("mime_type")]
    public string? MimeType { get; set; }

    [JsonProperty("caption")]
    public string? Caption { get; set; }
}

public class WhatsAppLocation
{
    [JsonProperty("latitude")]
    public double Latitude { get; set; }

    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("address")]
    public string? Address { get; set; }
}

public class WhatsAppStatus
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("status")]
    public string Status { get; set; } = null!;
}

// Media retrieval response

public class WhatsAppMediaResponse
{
    [JsonProperty("url")]
    public string Url { get; set; } = null!;

    [JsonProperty("mime_type")]
    public string MimeType { get; set; } = null!;
}

// Outgoing message payload

internal class WhatsAppSendMessageRequest
{
    [JsonProperty("messaging_product")]
    public string MessagingProduct { get; set; } = "whatsapp";

    [JsonProperty("recipient_type")]
    public string RecipientType { get; set; } = "individual";

    [JsonProperty("to")]
    public string To { get; set; } = null!;

    [JsonProperty("type")]
    public string Type { get; set; } = "text";

    [JsonProperty("text")]
    public WhatsAppSendText? Text { get; set; }
}

internal class WhatsAppSendText
{
    [JsonProperty("preview_url")]
    public bool PreviewUrl { get; set; }

    [JsonProperty("body")]
    public string Body { get; set; } = null!;
}
