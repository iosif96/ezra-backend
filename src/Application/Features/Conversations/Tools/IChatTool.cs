using Application.Common.Models.Chat;

using Newtonsoft.Json.Linq;

namespace Application.Features.Conversations.Tools;

public interface IChatTool
{
    string Name { get; }
    string Description { get; }
    JObject InputSchema { get; }

    bool IsAvailable(ToolContext context) => true;

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
    public int? IdentityId { get; set; }
}
