using Microsoft.AspNetCore.Http;

namespace Application.Common.Models.Gemini;

/// <summary>
/// Represents a chat message in the Gemini conversation
/// </summary>
public class ChatMessage
{
    public ChatRole Role { get; set; }
    public string? Content { get; set; }
    public List<IFormFile>? Attachments { get; set; }

    public ChatMessage(ChatRole role, string? content, List<IFormFile>? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(content) && (attachments == null || !attachments.Any()))
        {
            throw new ArgumentException("A message must have either content or attachments.");
        }

        Role = role;
        Content = content;
        Attachments = attachments;
    }
}

public enum ChatRole
{
    User,
    Model,
    System
}

/// <summary>
/// Configuration options for a Gemini API request
/// </summary>
public class GeminiOptions
{
    public bool EnableGrounding { get; set; }
    public bool EnableUrlContext { get; set; }
    public object? JsonSchema { get; set; }
    public string? SystemPrompt { get; set; }
    public string Model { get; set; } = "gemini-2.0-flash-exp";
    public int? MaxTokens { get; set; }
    public double? Temperature { get; set; }
}

/// <summary>
/// Request wrapper for Gemini API calls
/// </summary>
public class GeminiChatRequest
{
    public List<ChatMessage> Messages { get; set; } = new();
    public GeminiOptions Options { get; set; } = new();

    public GeminiChatRequest()
    {
    }

    public GeminiChatRequest(string message)
    {
        Messages.Add(new ChatMessage(ChatRole.User, message));
    }

    public GeminiChatRequest(List<ChatMessage> messages, GeminiOptions? options = null)
    {
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        Options = options ?? new GeminiOptions();
    }
}

/// <summary>
/// Response wrapper for Gemini API calls
/// </summary>
public class GeminiChatResponse
{
    public string Content { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FinishReason { get; set; }
}