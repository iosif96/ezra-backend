using Application.Features.Conversations.Prompts;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;

namespace Application.Features.Conversations.Tools;

public class BindIdentityTool(ApplicationDbContext context) : IChatTool
{
    public string Name => "bind_identity";
    public string UserLabel => "Boarding pass verified";

    public string Description =>
        "Looks up a boarding pass by its code and binds the passenger's identity to this conversation. " +
        "Use this when the passenger provides their boarding pass code or you extract it from a boarding pass photo. " +
        "This lets you know who the passenger is and access their flight details.";

    public JObject InputSchema => JObject.Parse("""
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
        """);

    public async Task<string> ExecuteAsync(JObject input, ToolContext toolContext, CancellationToken cancellationToken = default)
    {
        var code = input.Value<string>("boarding_pass_code")
            ?? throw new ArgumentException("Missing 'boarding_pass_code' property");

        var boardingPass = await context.BoardingPasses
            .Include(bp => bp.Identity)
            .Include(bp => bp.Flight)
                .ThenInclude(f => f.Movements)
                    .ThenInclude(m => m.Airport)
                        .ThenInclude(a => a.Terminals)
                            .ThenInclude(t => t.Gates)
            .Include(bp => bp.Flight)
                .ThenInclude(f => f.Movements)
                    .ThenInclude(m => m.Airport)
                        .ThenInclude(a => a.Terminals)
                            .ThenInclude(t => t.Merchants)
            .Include(bp => bp.Flight)
                .ThenInclude(f => f.Movements)
                    .ThenInclude(m => m.Terminal)
            .Include(bp => bp.Flight)
                .ThenInclude(f => f.Movements)
                    .ThenInclude(m => m.Gate)
            .FirstOrDefaultAsync(bp => bp.Code == code, cancellationToken);

        if (boardingPass is null)
        {
            return $"No boarding pass found with code '{code}'.";
        }

        // Bind the passenger identity to this conversation
        var conversation = await context.Conversations.FindAsync([toolContext.ConversationId], cancellationToken);
        if (conversation is null)
        {
            return "Error: conversation not found.";
        }

        conversation.IdentityId = boardingPass.IdentityId;
        await context.SaveChangesAsync(cancellationToken);

        var identity = boardingPass.Identity;
        var flight = boardingPass.Flight;

        var result = $"Passenger: {identity.PassengerName ?? "Unknown"}, Seat: {boardingPass.Seat ?? "N/A"}";
        result += $"\n{InfoBuilder.BuildFlightInfo(flight)}";

        // Include info for each airport involved in the flight
        var airports = flight.Movements
            .Select(m => m.Airport)
            .DistinctBy(a => a.Id);

        foreach (var airport in airports)
        {
            result += $"\n{InfoBuilder.BuildAirportInfo(airport)}";
        }

        return result;
    }
}
