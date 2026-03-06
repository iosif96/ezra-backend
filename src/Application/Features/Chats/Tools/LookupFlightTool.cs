using System.Text.Json;

using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats.Tools;

public class LookupFlightTool(ApplicationDbContext context) : IChatTool
{
    public string Name => "lookup_flight";

    public string Description =>
        "Searches for a flight by flight number (e.g. 'W6 3121', 'BA 283'). " +
        "Returns flight details including status, departure/arrival times, terminals, and gates.";

    public JsonElement InputSchema => JsonDocument.Parse("""
        {
            "type": "object",
            "properties": {
                "flight_number": {
                    "type": "string",
                    "description": "The flight number to search for."
                }
            },
            "required": ["flight_number"]
        }
        """).RootElement.Clone();

    public async Task<string> ExecuteAsync(JsonElement input, ToolContext toolContext, CancellationToken cancellationToken = default)
    {
        var flightNumber = input.GetProperty("flight_number").GetString()
            ?? throw new ArgumentException("Missing 'flight_number' property");

        var normalized = flightNumber.Replace(" ", "").ToUpper();

        var flight = await context.Flights
            .Include(f => f.Movements)
                .ThenInclude(m => m.Airport)
            .Include(f => f.Movements)
                .ThenInclude(m => m.Terminal)
            .Include(f => f.Movements)
                .ThenInclude(m => m.Gate)
            .FirstOrDefaultAsync(f =>
                f.Number.Replace(" ", "").ToUpper() == normalized ||
                f.IataCode.ToUpper() == normalized,
                cancellationToken);

        if (flight is null)
            return $"No flight found for '{flightNumber}'.";

        return ChatInfoBuilder.BuildFlightInfo(flight);
    }
}
