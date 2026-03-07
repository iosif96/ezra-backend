using Application.Features.Conversations.Prompts;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;

namespace Application.Features.Conversations.Tools;

public class LookupFlightTool(ApplicationDbContext context) : IChatTool
{
    public string Name => "lookup_flight";

    public string Description =>
        "Searches for a flight by flight number (e.g. 'W6 3121', 'BA 283'). " +
        "Returns flight details including status, departure/arrival times, terminals, and gates.";

    public JObject InputSchema => JObject.Parse("""
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
        """);

    public async Task<string> ExecuteAsync(JObject input, ToolContext toolContext, CancellationToken cancellationToken = default)
    {
        var flightNumber = input.Value<string>("flight_number")
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
        {
            return $"No flight found for '{flightNumber}'.";
        }

        return InfoBuilder.BuildFlightInfo(flight);
    }
}
