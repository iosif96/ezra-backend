using Newtonsoft.Json;

namespace Application.Common.Models.Gemini;

public class GeminiApiResponse
{
    [JsonProperty("candidates")]
    public List<CandidateDto>? Candidates { get; set; }

    [JsonProperty("promptFeedback")]
    public PromptFeedbackDto? PromptFeedback { get; set; }
}

public class PromptFeedbackDto
{
    [JsonProperty("blockReason")]
    public string? BlockReason { get; set; }
}

public class CandidateDto
{
    [JsonProperty("content")]
    public ContentDto? Content { get; set; }

    [JsonProperty("finishReason")]
    public string? FinishReason { get; set; }
}