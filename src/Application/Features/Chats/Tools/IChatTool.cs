using System.Text.Json;

using Application.Common.Models.Chat;

namespace Application.Features.Chats.Tools;

public interface IChatTool
{
    string Name { get; }
    string Description { get; }
    JsonElement InputSchema { get; }

    Task<string> ExecuteAsync(JsonElement input, ToolContext context, CancellationToken cancellationToken = default);

    ChatToolDefinition ToDefinition() => new()
    {
        Name = Name,
        Description = Description,
        InputSchema = InputSchema,
    };
}

public class ToolContext
{
    public required int ConversationId { get; set; }
}
