using System.Text.Json;

using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats.Tools;

public class BindIdentityTool(ApplicationDbContext context) : IChatTool
{
    public string Name => "bind_identity";

    public string Description =>
        "Looks up a boarding pass by its code and binds the passenger's identity to this conversation. " +
        "Use this when the passenger provides their boarding pass code or you extract it from a boarding pass photo. " +
        "This lets you know who the passenger is and access their flight details.";

    public JsonElement InputSchema => JsonDocument.Parse("""
        {
            "type": "object",
            "properties": {
                "boarding_pass_code": {
                    "type": "string",
                    "description": "The boarding pass code to look up."
                }
            },
            "required": ["boarding_pass_code"]
        }
        """).RootElement.Clone();

    public async Task<string> ExecuteAsync(JsonElement input, ToolContext toolContext, CancellationToken cancellationToken = default)
    {
        var code = input.GetProperty("boarding_pass_code").GetString()
            ?? throw new ArgumentException("Missing 'boarding_pass_code' property");

        var boardingPass = await context.BoardingPasses
            .Include(bp => bp.Identity)
            .Include(bp => bp.Flight)
                .ThenInclude(f => f.Movements)
                    .ThenInclude(m => m.Gate)
            .Include(bp => bp.Flight)
                .ThenInclude(f => f.Movements)
                    .ThenInclude(m => m.Terminal)
            .Include(bp => bp.Flight)
                .ThenInclude(f => f.Movements)
                    .ThenInclude(m => m.Airport)
            .FirstOrDefaultAsync(bp => bp.Code == code, cancellationToken);

        if (boardingPass is null)
            return $"No boarding pass found with code '{code}'.";

        var conversation = await context.Conversations
            .FirstOrDefaultAsync(c => c.Id == toolContext.ConversationId, cancellationToken);

        if (conversation is null)
            return "Error: conversation not found.";

        conversation.IdentityId = boardingPass.IdentityId;
        await context.SaveChangesAsync(cancellationToken);

        var identity = boardingPass.Identity;

        return $"Identity bound. Passenger: {identity.PassengerName ?? "Unknown"}, Seat: {boardingPass.Seat ?? "N/A"}\n{ChatInfoBuilder.BuildFlightInfo(boardingPass.Flight)}";
    }
}
