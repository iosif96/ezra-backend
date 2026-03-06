using System.Text;

using Application.Common.Interfaces;
using Application.Common.Models.Gemini;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Infrastructure.Services;

/// <summary>
/// Service implementation for Google's Gemini AI API
/// </summary>
public class GeminiService : IGeminiService
{
    private const string BaseApiUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private const int DefaultTimeoutMinutes = 10;
    private const int MaxRetries = 1;

    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSettings;

    public GeminiService(
        IConfiguration configuration,
        ILogger<GeminiService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiKey = configuration["GeminiConfiguration:Key"]
            ?? throw new InvalidOperationException("Gemini API key not configured");

        _httpClient = httpClientFactory.CreateClient("GeminiClient");
        _httpClient.Timeout = TimeSpan.FromMinutes(DefaultTimeoutMinutes);

        _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };
    }

    /// <inheritdoc />
    public async Task<GeminiChatResponse> SendChatAsync(
        GeminiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.Messages.Any())
        {
            throw new ArgumentException("Chat request must contain at least one message", nameof(request));
        }

        try
        {
            var apiRequest = await BuildApiRequestAsync(request, cancellationToken);
            var apiResponse = await SendRequestWithRetryAsync(apiRequest, request.Options.Model, cancellationToken);
            return ProcessApiResponse(apiResponse, request.Options.JsonSchema != null);
        }
        catch (GeminiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Gemini service");
            throw new GeminiException("An unexpected error occurred while processing the request", ex);
        }
    }

    /// <inheritdoc />
    public async Task<string> SendMessageAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }

        var request = new GeminiChatRequest(message);
        var response = await SendChatAsync(request, cancellationToken);

        if (!response.IsSuccess)
        {
            throw new GeminiException(response.ErrorMessage ?? "Request failed");
        }

        return response.Content;
    }

    private async Task<GeminiApiRequest> BuildApiRequestAsync(
        GeminiChatRequest request,
        CancellationToken cancellationToken)
    {
        var apiRequest = new GeminiApiRequest();

        // Add system instruction if provided
        if (!string.IsNullOrWhiteSpace(request.Options.SystemPrompt))
        {
            apiRequest.SystemInstruction = new ContentDto
            {
                Parts = new List<PartDto>
                {
                    new() { Text = request.Options.SystemPrompt },
                },
            };
        }

        // Convert chat messages to API format
        foreach (var message in request.Messages)
        {
            // Skip system messages as they're handled separately
            if (message.Role == ChatRole.System)
            {
                continue;
            }

            var parts = await CreatePartsAsync(message, cancellationToken);
            if (parts.Any())
            {
                apiRequest.Contents.Add(new ContentDto
                {
                    Role = message.Role == ChatRole.User ? "user" : "model",
                    Parts = parts,
                });
            }
        }

        // Configure tools
        ConfigureTools(apiRequest, request.Options);

        // Configure generation settings
        ConfigureGeneration(apiRequest, request.Options);

        return apiRequest;
    }

    private async Task<List<PartDto>> CreatePartsAsync(
        ChatMessage message,
        CancellationToken cancellationToken)
    {
        var parts = new List<PartDto>();

        // Add text content
        if (!string.IsNullOrWhiteSpace(message.Content))
        {
            parts.Add(new PartDto { Text = message.Content });
        }

        // Add file attachments
        if (message.Attachments?.Any() == true)
        {
            foreach (var file in message.Attachments)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var base64Data = await ConvertFileToBase64Async(file, cancellationToken);
                    parts.Add(new PartDto
                    {
                        InlineData = new InlineDataDto
                        {
                            MimeType = file.ContentType ?? GetMimeType(file.FileName),
                            Data = base64Data
                        }
                    });

                    _logger.LogDebug("Added attachment {FileName} to request", file.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process attachment {FileName}", file.FileName);
                    // Continue with other attachments
                }
            }
        }

        return parts;
    }

    private static async Task<string> ConvertFileToBase64Async(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            _ => "application/octet-stream"
        };
    }

    private static void ConfigureTools(GeminiApiRequest request, GeminiOptions options)
    {
        var tools = new List<ToolDto>();

        if (options.EnableGrounding)
        {
            tools.Add(new ToolDto { GoogleSearchRetrieval = new Dictionary<string, object>() });
        }

        if (options.EnableUrlContext)
        {
            tools.Add(new ToolDto { UrlContext = new Dictionary<string, object>() });
        }

        if (tools.Any())
        {
            request.Tools = tools;
        }
    }

    private void ConfigureGeneration(GeminiApiRequest request, GeminiOptions options)
    {
        var config = new GenerationConfigDto();
        var hasConfig = false;

        // Configure JSON response if schema provided
        if (options.JsonSchema != null)
        {
            try
            {
                config.ResponseMimeType = "application/json";
                config.ResponseSchema = options.JsonSchema;
                hasConfig = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to configure JSON schema");
            }
        }

        // Configure other generation parameters
        if (options.MaxTokens.HasValue)
        {
            config.MaxOutputTokens = options.MaxTokens.Value;
            hasConfig = true;
        }

        if (options.Temperature.HasValue)
        {
            config.Temperature = options.Temperature.Value;
            hasConfig = true;
        }

        if (hasConfig)
        {
            request.GenerationConfig = config;
        }
    }

    private async Task<GeminiApiResponse> SendRequestWithRetryAsync(
        GeminiApiRequest request,
        string model,
        CancellationToken cancellationToken)
    {
        var url = $"{BaseApiUrl}/{model}:generateContent?key={_apiKey}";
        var json = JsonConvert.SerializeObject(request, _jsonSettings);

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    _logger.LogDebug("Retrying request after {Delay} seconds (attempt {Attempt}/{MaxRetries})",
                        delay.TotalSeconds, attempt + 1, MaxRetries);
                    await Task.Delay(delay, cancellationToken);
                }

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync(url, content, cancellationToken);

                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return DeserializeResponse(responseText);
                }

                // Handle specific error codes
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (attempt < MaxRetries - 1)
                    {
                        continue; // Retry
                    }
                    throw new GeminiRateLimitException();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new GeminiException("Invalid API key", "UNAUTHORIZED", 401);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new GeminiException($"Invalid request: {responseText}", "BAD_REQUEST", 400);
                }

                // For other errors, retry if we haven't exceeded max attempts
                if (attempt < MaxRetries - 1)
                {
                    _logger.LogWarning("API request failed with status {StatusCode}, retrying...", response.StatusCode);
                    continue;
                }

                throw new GeminiException(
                    $"API request failed with status {response.StatusCode}: {responseText}",
                    response.StatusCode.ToString(),
                    (int)response.StatusCode);
            }
            catch (TaskCanceledException)
            {
                throw new GeminiException("Request timeout", "TIMEOUT");
            }
            catch (HttpRequestException ex)
            {
                if (attempt < MaxRetries - 1)
                {
                    _logger.LogWarning(ex, "Network error, retrying...");
                    continue;
                }
                throw new GeminiException("Network error occurred", ex, "NETWORK_ERROR");
            }
        }

        throw new GeminiException("Maximum retry attempts exceeded", "MAX_RETRIES");
    }

    private GeminiApiResponse DeserializeResponse(string responseText)
    {
        try
        {
            return JsonConvert.DeserializeObject<GeminiApiResponse>(responseText)
                ?? throw new GeminiException("Empty response from API");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize API response");
            throw new GeminiException("Invalid response format from API", ex, "DESERIALIZATION_ERROR");
        }
    }

    private GeminiChatResponse ProcessApiResponse(GeminiApiResponse apiResponse, bool expectJson)
    {
        var response = new GeminiChatResponse();

        // Check if request was blocked
        if (apiResponse.PromptFeedback?.BlockReason != null)
        {
            _logger.LogWarning("Request blocked: {Reason}", apiResponse.PromptFeedback.BlockReason);
            throw new GeminiSafetyException(apiResponse.PromptFeedback.BlockReason);
        }

        // Get first candidate
        var candidate = apiResponse.Candidates?.FirstOrDefault();
        if (candidate == null)
        {
            response.IsSuccess = false;
            response.ErrorMessage = "No response generated";
            return response;
        }

        // Set finish reason
        response.FinishReason = candidate.FinishReason;

        // Check finish reason for issues
        if (candidate.FinishReason is "SAFETY" or "RECITATION")
        {
            response.IsSuccess = false;
            response.ErrorMessage = $"Generation stopped: {candidate.FinishReason}";
            return response;
        }

        // Extract text content
        var textContent = candidate.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrEmpty(textContent))
        {
            response.IsSuccess = false;
            response.ErrorMessage = "No text content in response";
            return response;
        }

        // Validate JSON if expected
        if (expectJson)
        {
            try
            {
                JToken.Parse(textContent);
            }
            catch (JsonException)
            {
                response.IsSuccess = false;
                response.ErrorMessage = "Response is not valid JSON";
                return response;
            }
        }

        response.Content = textContent;
        response.IsSuccess = true;
        return response;
    }
}