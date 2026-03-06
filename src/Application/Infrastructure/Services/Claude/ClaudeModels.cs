using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Application.Infrastructure.Services.Claude;

internal class ClaudeRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = null!;

    [JsonProperty("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
    public string? System { get; set; }

    [JsonProperty("temperature", NullValueHandling = NullValueHandling.Ignore)]
    public double? Temperature { get; set; }

    [JsonProperty("messages")]
    public List<ClaudeMessage> Messages { get; set; } = [];

    [JsonProperty("tools", NullValueHandling = NullValueHandling.Ignore)]
    public List<ClaudeTool>? Tools { get; set; }
}

internal class ClaudeMessage
{
    [JsonProperty("role")]
    public string Role { get; set; } = null!;

    [JsonProperty("content")]
    public JToken Content { get; set; } = null!;
}

internal class ClaudeTool
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("description")]
    public string Description { get; set; } = null!;

    [JsonProperty("input_schema")]
    public JToken InputSchema { get; set; } = null!;
}

internal class ClaudeResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("content")]
    public List<ClaudeContentBlock> Content { get; set; } = [];

    [JsonProperty("stop_reason")]
    public string StopReason { get; set; } = null!;

    [JsonProperty("usage")]
    public ClaudeUsage Usage { get; set; } = null!;
}

internal class ClaudeContentBlock
{
    [JsonProperty("type")]
    public string Type { get; set; } = null!;

    [JsonProperty("text")]
    public string? Text { get; set; }

    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("input")]
    public JObject? Input { get; set; }
}

internal class ClaudeUsage
{
    [JsonProperty("input_tokens")]
    public int InputTokens { get; set; }

    [JsonProperty("output_tokens")]
    public int OutputTokens { get; set; }
}

internal class ClaudeErrorResponse
{
    [JsonProperty("error")]
    public ClaudeError? Error { get; set; }
}

internal class ClaudeError
{
    [JsonProperty("type")]
    public string Type { get; set; } = null!;

    [JsonProperty("message")]
    public string Message { get; set; } = null!;
}
