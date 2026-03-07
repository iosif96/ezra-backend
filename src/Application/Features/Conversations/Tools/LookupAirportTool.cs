using Application.Features.Conversations.Prompts;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;

namespace Application.Features.Conversations.Tools;

public class LookupAirportTool(ApplicationDbContext context) : IChatTool
{
    public string Name => "lookup_airport";

    public string Description =>
        "Searches for an airport by IATA code (e.g. 'OTP', 'LHR'), city name (e.g. 'Oradea', 'London'), " +
        "or airport name, and returns its information including terminals, gates, and merchants.";

    public JObject InputSchema => JObject.Parse("""
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
        """);

    public async Task<string> ExecuteAsync(JObject input, ToolContext toolContext, CancellationToken cancellationToken = default)
    {
        var query = input.Value<string>("query")
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
        {
            return $"No airport found for '{query}'.";
        }

        return InfoBuilder.BuildAirportInfo(airport);
    }
}
