using Application.Common.Interfaces;
using Application.Common.Models.AviationStack;
using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Flights.SyncFlights;

public record SyncFlightsResponse(int Created, int Updated);

public record SyncFlightsCommand(int AirportId) : IRequest<SyncFlightsResponse>;

internal sealed class SyncFlightsCommandHandler(
    ApplicationDbContext context,
    IAviationStackService aviationStack,
    ILogger<SyncFlightsCommandHandler> logger) : IRequestHandler<SyncFlightsCommand, SyncFlightsResponse>
{
    public async Task<SyncFlightsResponse> Handle(SyncFlightsCommand request, CancellationToken cancellationToken)
    {
        var airport = await context.Airports
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AirportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), request.AirportId);

        // Fetch departures + arrivals from AviationStack
        var fetched = new List<(AviationStackFlight Flight, MovementType Direction)>();

        foreach (var direction in new[] { MovementType.Departure, MovementType.Arrival })
        {
            var flights = await aviationStack.GetFlightsAsync(airport.IataCode, direction, cancellationToken);
            fetched.AddRange(flights.Select(f => (f, direction)));
        }

        // Deduplicate: a flight can appear in both dep and arr queries
        var parsed = fetched
            .Select(f => ParseFlight(f.Flight, f.Direction))
            .Where(f => f != null)
            .GroupBy(f => (f!.FlightNumber, f.FlightDate))
            .Select(g => MergeParsed(g!))
            .ToList();

        logger.LogInformation("Syncing {Count} unique flights for airport {Airport}", parsed.Count, airport.IataCode);

        // Batch-load existing flights to avoid N+1 queries
        var flightNumbers = parsed.Select(p => p.FlightNumber).Distinct().ToList();
        var dates = parsed.Select(p => p.FlightDate).Distinct().ToList();

        var existingFlights = await context.Flights
            .Include(f => f.Movements)
            .Where(f => flightNumbers.Contains(f.Number) && dates.Contains(f.Date))
            .ToListAsync(cancellationToken);

        // Group by key — handles duplicates in DB safely
        var existingLookup = existingFlights
            .GroupBy(f => (f.Number, f.Date))
            .ToDictionary(g => g.Key, g => g.First());

        // Pre-load terminals and gates for the airport to resolve AviationStack string codes
        var terminals = await context.Set<Terminal>()
            .Where(t => t.AirportId == airport.Id)
            .ToDictionaryAsync(t => t.Code, t => t.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var gates = await context.Set<Gate>()
            .Include(g => g.Terminal)
            .Where(g => g.Terminal.AirportId == airport.Id)
            .ToDictionaryAsync(g => g.Code, g => g.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var created = 0;
        var updated = 0;

        foreach (var p in parsed)
        {
            if (existingLookup.TryGetValue((p.FlightNumber, p.FlightDate), out var existing))
            {
                ApplyUpdate(existing, p);
                UpsertMovement(existing, p, airport.Id, terminals, gates);
                updated++;
            }
            else
            {
                var flight = CreateFlight(p);
                AddMovement(flight, p, airport.Id, terminals, gates);
                context.Flights.Add(flight);
                created++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Sync complete for {Airport}: {Created} created, {Updated} updated",
            airport.IataCode, created, updated);

        return new SyncFlightsResponse(created, updated);
    }

    private sealed class ParsedFlight
    {
        public required string FlightNumber { get; init; }
        public required DateOnly FlightDate { get; init; }
        public string IataCode { get; set; } = string.Empty;
        public string IcaoCode { get; set; } = string.Empty;
        public string Airline { get; set; } = string.Empty;
        public FlightStatus Status { get; set; } = FlightStatus.Unknown;
        public AviationStackMovement? DepartureData { get; set; }
        public AviationStackMovement? ArrivalData { get; set; }
    }

    private static ParsedFlight? ParseFlight(AviationStackFlight av, MovementType direction)
    {
        var flightNumber = av.Flight.Iata ?? av.Flight.Icao ?? av.Flight.Number;
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            return null;
        }

        if (!DateOnly.TryParse(av.FlightDate, out var flightDate))
        {
            return null;
        }

        var parsed = new ParsedFlight
        {
            FlightNumber = flightNumber,
            FlightDate = flightDate,
            IataCode = av.Flight.Iata ?? string.Empty,
            IcaoCode = av.Flight.Icao ?? string.Empty,
            Airline = av.Airline.Name ?? string.Empty,
            Status = MapStatus(av.FlightStatus),
        };

        if (direction == MovementType.Departure)
        {
            parsed.DepartureData = av.Departure;
        }
        else
        {
            parsed.ArrivalData = av.Arrival;
        }

        return parsed;
    }

    private static ParsedFlight MergeParsed(IGrouping<(string, DateOnly), ParsedFlight?> group)
    {
        var items = group.Where(x => x != null).ToList();
        var first = items.First()!;

        foreach (var other in items.Skip(1))
        {
            first.DepartureData ??= other!.DepartureData;
            first.ArrivalData ??= other!.ArrivalData;

            if (!string.IsNullOrEmpty(other!.IataCode))
            {
                first.IataCode = other.IataCode;
            }

            if (!string.IsNullOrEmpty(other.IcaoCode))
            {
                first.IcaoCode = other.IcaoCode;
            }

            if (!string.IsNullOrEmpty(other.Airline))
            {
                first.Airline = other.Airline;
            }

            if (other.Status != FlightStatus.Unknown)
            {
                first.Status = other.Status;
            }
        }

        return first;
    }

    private static Flight CreateFlight(ParsedFlight p)
    {
        return new Flight
        {
            Date = p.FlightDate,
            Number = p.FlightNumber,
            IataCode = p.IataCode,
            IcaoCode = p.IcaoCode,
            Airline = p.Airline,
            Status = p.Status,
            FlightSource = FlightSource.AviationStack,
        };
    }

    private static void ApplyUpdate(Flight flight, ParsedFlight p)
    {
        flight.Status = p.Status;

        if (!string.IsNullOrEmpty(p.IataCode))
        {
            flight.IataCode = p.IataCode;
        }

        if (!string.IsNullOrEmpty(p.IcaoCode))
        {
            flight.IcaoCode = p.IcaoCode;
        }

        if (!string.IsNullOrEmpty(p.Airline))
        {
            flight.Airline = p.Airline;
        }
    }

    private static void AddMovement(Flight flight, ParsedFlight p, int airportId,
        Dictionary<string, int> terminals, Dictionary<string, int> gates)
    {
        if (p.DepartureData?.Scheduled != null)
        {
            flight.Movements.Add(BuildMovement(MovementType.Departure, p.DepartureData, airportId, terminals, gates));
        }

        if (p.ArrivalData?.Scheduled != null)
        {
            flight.Movements.Add(BuildMovement(MovementType.Arrival, p.ArrivalData, airportId, terminals, gates));
        }
    }

    private static void UpsertMovement(Flight flight, ParsedFlight p, int airportId,
        Dictionary<string, int> terminals, Dictionary<string, int> gates)
    {
        if (p.DepartureData?.Scheduled != null)
        {
            UpsertSingleMovement(flight, MovementType.Departure, p.DepartureData, airportId, terminals, gates);
        }

        if (p.ArrivalData?.Scheduled != null)
        {
            UpsertSingleMovement(flight, MovementType.Arrival, p.ArrivalData, airportId, terminals, gates);
        }
    }

    private static void UpsertSingleMovement(Flight flight, MovementType type, AviationStackMovement data, int airportId,
        Dictionary<string, int> terminals, Dictionary<string, int> gates)
    {
        var existing = flight.Movements.FirstOrDefault(m => m.Type == type && m.AirportId == airportId);
        if (existing != null)
        {
            existing.ScheduledOn = data.Scheduled!.Value;
            existing.EstimatedOn = data.Estimated;
            existing.ActualOn = data.Actual;
            ResolveTerminalAndGate(existing, data, terminals, gates);
        }
        else
        {
            flight.Movements.Add(BuildMovement(type, data, airportId, terminals, gates));
        }
    }

    private static FlightMovement BuildMovement(MovementType type, AviationStackMovement data, int airportId,
        Dictionary<string, int> terminals, Dictionary<string, int> gates)
    {
        var movement = new FlightMovement
        {
            Type = type,
            AirportId = airportId,
            ScheduledOn = data.Scheduled!.Value,
            EstimatedOn = data.Estimated,
            ActualOn = data.Actual,
        };

        ResolveTerminalAndGate(movement, data, terminals, gates);

        return movement;
    }

    private static void ResolveTerminalAndGate(FlightMovement movement, AviationStackMovement data,
        Dictionary<string, int> terminals, Dictionary<string, int> gates)
    {
        if (!string.IsNullOrWhiteSpace(data.Terminal) && terminals.TryGetValue(data.Terminal, out var terminalId))
        {
            movement.TerminalId = terminalId;
        }

        if (!string.IsNullOrWhiteSpace(data.Gate) && gates.TryGetValue(data.Gate, out var gateId))
        {
            movement.GateId = gateId;
        }
    }

    private static FlightStatus MapStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "scheduled" => FlightStatus.Scheduled,
            "active" => FlightStatus.Active,
            "landed" => FlightStatus.Landed,
            "cancelled" => FlightStatus.Cancelled,
            "incident" => FlightStatus.Unknown,
            "diverted" => FlightStatus.Diverted,
            _ => FlightStatus.Unknown,
        };
    }
}
