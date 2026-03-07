using System.Text;

using Application.Common.Interfaces;
using Application.Common.Models.Chat;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Application.Infrastructure.Services.Claude;

public class ClaudeCompletionService : IChatCompletionService
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string ApiVersion = "2023-06-01";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ClaudeCompletionService> _logger;

    public ClaudeCompletionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ClaudeCompletionService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AnthropicConfiguration:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic API key not configured");
        _logger = logger;
    }

    public async Task<ChatCompletionResult> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var claudeRequest = MapToClaudeRequest(request);
        var json = JsonConvert.SerializeObject(claudeRequest);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        httpRequest.Headers.Add("x-api-key", _apiKey);
        httpRequest.Headers.Add("anthropic-version", ApiVersion);
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = JsonConvert.DeserializeObject<ClaudeErrorResponse>(responseText);
            _logger.LogError("Claude API error {StatusCode}: {Error}", response.StatusCode, error?.Error?.Message ?? responseText);
            throw new HttpRequestException($"Claude API error ({response.StatusCode}): {error?.Error?.Message ?? responseText}");
        }

        var claudeResponse = JsonConvert.DeserializeObject<ClaudeResponse>(responseText)
            ?? throw new InvalidOperationException("Empty response from Claude API");

        return MapFromClaudeResponse(claudeResponse);
    }

    private static ClaudeRequest MapToClaudeRequest(ChatCompletionRequest request)
    {
        var cacheBreakpoint = new JObject { ["type"] = "ephemeral" };

        var claudeRequest = new ClaudeRequest
        {
            Model = request.Model,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
        };

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            claudeRequest.System = new JArray
            {
                new JObject
                {
                    ["type"] = "text",
                    ["text"] = request.SystemPrompt,
                    ["cache_control"] = cacheBreakpoint,
                },
            };
        }

        foreach (var message in request.Messages)
        {
            var role = message.Role == ChatMessageRole.User ? "user" : "assistant";
            var contentBlocks = new JArray();

            foreach (var block in message.Content)
            {
                switch (block)
                {
                    case TextContent text:
                        contentBlocks.Add(new JObject
                        {
                            ["type"] = "text",
                            ["text"] = text.Text,
                        });
                        break;

                    case ImageContent image:
                        contentBlocks.Add(new JObject
                        {
                            ["type"] = "image",
                            ["source"] = new JObject
                            {
                                ["type"] = "base64",
                                ["media_type"] = image.MediaType,
                                ["data"] = image.Base64Data,
                            },
                        });
                        break;

                    case ToolUseContent toolUse:
                        contentBlocks.Add(new JObject
                        {
                            ["type"] = "tool_use",
                            ["id"] = toolUse.Id,
                            ["name"] = toolUse.Name,
                            ["input"] = toolUse.Input,
                        });
                        break;

                    case ToolResultContent toolResult:
                        contentBlocks.Add(new JObject
                        {
                            ["type"] = "tool_result",
                            ["tool_use_id"] = toolResult.ToolUseId,
                            ["content"] = toolResult.Content,
                            ["is_error"] = toolResult.IsError,
                        });
                        break;
                }
            }

            claudeRequest.Messages.Add(new ClaudeMessage
            {
                Role = role,
                Content = contentBlocks,
            });
        }

        if (request.Tools is { Count: > 0 })
        {
            claudeRequest.Tools = request.Tools.Select((t, i) => new ClaudeTool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.InputSchema,
                CacheControl = i == request.Tools.Count - 1 ? cacheBreakpoint : null,
            }).ToList();
        }

        return claudeRequest;
    }

    private static ChatCompletionResult MapFromClaudeResponse(ClaudeResponse response)
    {
        var content = new List<ContentBlock>();

        foreach (var block in response.Content)
        {
            switch (block.Type)
            {
                case "text":
                    content.Add(new TextContent { Text = block.Text! });
                    break;

                case "tool_use":
                    content.Add(new ToolUseContent
                    {
                        Id = block.Id!,
                        Name = block.Name!,
                        Input = block.Input ?? new JObject(),
                    });
                    break;
            }
        }

        return new ChatCompletionResult
        {
            Content = content,
            StopReason = response.StopReason,
            InputTokens = response.Usage.InputTokens,
            OutputTokens = response.Usage.OutputTokens,
        };
    }
}
