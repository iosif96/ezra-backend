using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Application.Common.Models.Gemini;

public class GeminiApiRequest
{
    [JsonProperty("contents")]
    public List<ContentDto> Contents { get; set; } = new();

    [JsonProperty("tools", NullValueHandling = NullValueHandling.Ignore)]
    public List<ToolDto>? Tools { get; set; }

    [JsonProperty("generationConfig", NullValueHandling = NullValueHandling.Ignore)]
    public GenerationConfigDto? GenerationConfig { get; set; }

    [JsonProperty("systemInstruction", NullValueHandling = NullValueHandling.Ignore)]
    public ContentDto? SystemInstruction { get; set; }
}

public class ContentDto
{
    [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
    public string? Role { get; set; }

    [JsonProperty("parts")]
    public List<PartDto> Parts { get; set; } = new();
}

public class PartDto
{
    [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
    public string? Text { get; set; }

    [JsonProperty("inlineData", NullValueHandling = NullValueHandling.Ignore)]
    public InlineDataDto? InlineData { get; set; }
}

public class InlineDataDto
{
    [JsonProperty("mimeType")]
    public string MimeType { get; set; } = "application/octet-stream";

    [JsonProperty("data")]
    public string Data { get; set; } = string.Empty;
}

public class ToolDto
{
    [JsonProperty("googleSearchRetrieval", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object>? GoogleSearchRetrieval { get; set; }

    [JsonProperty("urlContext", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object>? UrlContext { get; set; }
}

public class GenerationConfigDto
{
    [JsonProperty("responseMimeType", NullValueHandling = NullValueHandling.Ignore)]
    public string? ResponseMimeType { get; set; }

    [JsonProperty("responseSchema", NullValueHandling = NullValueHandling.Ignore)]
    public object? ResponseSchema { get; set; }

    [JsonProperty("maxOutputTokens", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxOutputTokens { get; set; }

    [JsonProperty("temperature", NullValueHandling = NullValueHandling.Ignore)]
    public double? Temperature { get; set; }
}