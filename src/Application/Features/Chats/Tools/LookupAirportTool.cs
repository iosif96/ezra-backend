using System.Text.Json;

using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats.Tools;

public class LookupAirportTool(ApplicationDbContext context) : IChatTool
{
    public string Name => "lookup_airport";

    public string Description =>
        "Searches for an airport by IATA code (e.g. 'OTP', 'LHR'), city name (e.g. 'Oradea', 'London'), " +
        "or airport name, and returns its information including terminals, gates, and merchants.";

    public JsonElement InputSchema => JsonDocument.Parse("""
        {
            "type": "object",
            "properties": {
                "query": {
                    "type": "string",
                    "description": "The IATA code, city name, or airport name to search for."
                }
            },
            "required": ["query"]
        }
        """).RootElement.Clone();

    public async Task<string> ExecuteAsync(JsonElement input, ToolContext toolContext, CancellationToken cancellationToken = default)
    {
        var query = input.GetProperty("query").GetString()
            ?? throw new ArgumentException("Missing 'query' property");

        var queryUpper = query.ToUpper();

        var airport = await context.Airports
            .Include(a => a.Terminals)
                .ThenInclude(t => t.Gates)
            .Include(a => a.Terminals)
                .ThenInclude(t => t.Merchants)
            .FirstOrDefaultAsync(a =>
                a.IataCode == queryUpper ||
                a.IcaoCode == queryUpper ||
                a.Name.ToUpper().Contains(queryUpper) ||
                a.City.ToUpper().Contains(queryUpper),
                cancellationToken);

        if (airport is null)
            return $"No airport found for '{query}'.";

        return ChatInfoBuilder.BuildAirportInfo(airport);
    }
}
