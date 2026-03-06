using System.Text.Json;

using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.Chats.Tools;

public class CreateRequestTool(ApplicationDbContext context) : IChatTool
{
    public string Name => "create_request";

    public string Description =>
        "Creates a service request on behalf of the passenger. " +
        "Use this when the passenger needs special assistance (wheelchair, mobility aid), " +
        "wants to be handed off to a human agent, or reports an emergency.";

    public JsonElement InputSchema => JsonDocument.Parse("""
        {
            "type": "object",
            "properties": {
                "type": {
                    "type": "string",
                    "enum": ["SpecialNeedsAssistance", "Handoff", "Emergency"],
                    "description": "The type of request. SpecialNeedsAssistance for wheelchair or mobility help, Handoff to transfer to a human agent, Emergency for urgent safety situations."
                },
                "content": {
                    "type": "string",
                    "description": "A brief description of what the passenger needs."
                }
            },
            "required": ["type", "content"]
        }
        """).RootElement.Clone();

    public async Task<string> ExecuteAsync(JsonElement input, ToolContext toolContext, CancellationToken cancellationToken = default)
    {
        var typeString = input.GetProperty("type").GetString()
            ?? throw new ArgumentException("Missing 'type' property");
        var content = input.GetProperty("content").GetString()
            ?? throw new ArgumentException("Missing 'content' property");

        if (!Enum.TryParse<RequestType>(typeString, out var requestType))
            return $"Error: Invalid request type '{typeString}'.";

        var entity = new Request
        {
            ConversationId = toolContext.ConversationId,
            Type = requestType,
            Content = content,
            Status = RequestStatus.Requested,
        };

        context.Requests.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return $"Request #{entity.Id} created successfully. Type: {requestType}, Status: Requested.";
    }
}
