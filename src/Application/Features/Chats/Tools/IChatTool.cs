using Application.Common.Models.Chat;

using Newtonsoft.Json.Linq;

namespace Application.Features.Chats.Tools;

public interface IChatTool
{
    string Name { get; }
    string Description { get; }
    JObject InputSchema { get; }

    Task<string> ExecuteAsync(JObject input, ToolContext context, CancellationToken cancellationToken = default);

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
