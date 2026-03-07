using Application.Common.Interfaces;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;

namespace Application.Features.Conversations.Tools;

public class CreateRequestTool(ApplicationDbContext context, IOverviewNotifier overviewNotifier) : IChatTool
{
    public string Name => "create_request";

    public string Description =>
        "Creates a service request on behalf of the passenger. " +
        "Use this when the passenger needs special assistance (wheelchair, mobility aid), " +
        "wants to be handed off to a human agent, or reports an emergency.";

    public bool IsAvailable(ToolContext context) => context.IdentityId is not null;

    public JObject InputSchema => JObject.Parse("""
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
        """);

    public async Task<string> ExecuteAsync(JObject input, ToolContext toolContext, CancellationToken cancellationToken = default)
    {
        var typeString = input.Value<string>("type")
            ?? throw new ArgumentException("Missing 'type' property");
        var content = input.Value<string>("content")
            ?? throw new ArgumentException("Missing 'content' property");

        if (!Enum.TryParse<RequestType>(typeString, out var requestType))
        {
            return $"Error: Invalid request type '{typeString}'.";
        }

        var entity = new Request
        {
            ConversationId = toolContext.ConversationId,
            Type = requestType,
            Content = content,
            Status = RequestStatus.Requested,
        };

        context.Requests.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var channelType = await context.Conversations
            .Where(c => c.Id == toolContext.ConversationId)
            .Select(c => c.ChannelType)
            .FirstAsync(cancellationToken);

        await overviewNotifier.NotifyRequestCreated(new RequestCreatedNotification(
            entity.Id, entity.Type, entity.Content, entity.Status, channelType, entity.Created), cancellationToken);

        return $"Request #{entity.Id} created successfully. Type: {requestType}, Status: Requested.";
    }
}
