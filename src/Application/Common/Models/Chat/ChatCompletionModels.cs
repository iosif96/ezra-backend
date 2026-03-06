using System.Text.Json;

namespace Application.Common.Models.Chat;

public class ChatCompletionRequest
{
    public required string Model { get; set; }
    public string? SystemPrompt { get; set; }
    public required List<ChatMessageDto> Messages { get; set; }
    public List<ChatToolDefinition>? Tools { get; set; }
    public int MaxTokens { get; set; } = 4096;
    public double? Temperature { get; set; }
}

public class ChatMessageDto
{
    public required ChatMessageRole Role { get; set; }
    public required List<ContentBlock> Content { get; set; }

    public static ChatMessageDto FromText(ChatMessageRole role, string text) => new()
    {
        Role = role,
        Content = [new TextContent { Text = text }],
    };
}

public enum ChatMessageRole
{
    User,
    Assistant,
}

public abstract class ContentBlock
{
    public abstract string Type { get; }
}

public class TextContent : ContentBlock
{
    public override string Type => "text";
    public required string Text { get; set; }
}

public class ImageContent : ContentBlock
{
    public override string Type => "image";
    public required string MediaType { get; set; }
    public required string Base64Data { get; set; }
}

public class ToolUseContent : ContentBlock
{
    public override string Type => "tool_use";
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required JsonElement Input { get; set; }
}

public class ToolResultContent : ContentBlock
{
    public override string Type => "tool_result";
    public required string ToolUseId { get; set; }
    public required string Content { get; set; }
    public bool IsError { get; set; }
}

public class ChatToolDefinition
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required JsonElement InputSchema { get; set; }
}

public class ChatCompletionResult
{
    public required List<ContentBlock> Content { get; set; }
    public required string StopReason { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }

    public string? GetText() => Content
        .OfType<TextContent>()
        .Select(t => t.Text)
        .FirstOrDefault();

    public List<ToolUseContent> GetToolCalls() => Content
        .OfType<ToolUseContent>()
        .ToList();

    public bool RequiresToolUse => StopReason == "tool_use";
}
