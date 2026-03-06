using Application.Common.Interfaces;
using Application.Common.Models.AviationStack;
using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Flights.SyncFlights;

public record SyncFlightsResponse(int Created, int Updated, int Skipped);

[Authorize]
public record SyncFlightsCommand(int AirportId) : IRequest<SyncFlightsResponse>;

internal sealed class SyncFlightsCommandHandler(
    ApplicationDbContext context,
    IAviationStackService aviationStack,
    IDateTime dateTime,
    ILogger<SyncFlightsCommandHandler> logger) : IRequestHandler<SyncFlightsCommand, SyncFlightsResponse>
{
    public async Task<SyncFlightsResponse> Handle(SyncFlightsCommand request, CancellationToken cancellationToken)
    {
        var airport = await context.Airports
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AirportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), request.AirportId);

        // Fetch today + tomorrow, departures + arrivals from AviationStack
        var today = DateOnly.FromDateTime(dateTime.Now);
        var tomorrow = today.AddDays(1);

        var fetched = new List<(AviationStackFlight Flight, MovementType Direction)>();

        foreach (var date in new[] { today, tomorrow })
        {
            foreach (var direction in new[] { MovementType.Departure, MovementType.Arrival })
            {
                var flights = await aviationStack.GetFlightsAsync(airport.IataCode, date, direction, cancellationToken);
                fetched.AddRange(flights.Select(f => (f, direction)));
            }
        }

        // A flight can appear in both dep and arr queries — deduplicate and merge movements
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

        var existingLookup = (await context.Flights
            .Include(f => f.Movements)
            .Where(f => flightNumbers.Contains(f.Number) && dates.Contains(f.Date))
            .ToListAsync(cancellationToken))
            .ToDictionary(f => (f.Number, f.Date));

        var created = 0;
        var updated = 0;
        var skipped = 0;

        foreach (var p in parsed)
        {
            if (existingLookup.TryGetValue((p.FlightNumber, p.FlightDate), out var existing))
            {
                ApplyUpdate(existing, p);
                UpsertMovement(existing, p, airport.Id);
                updated++;
            }
            else
            {
                var flight = CreateFlight(p);
                AddMovement(flight, p, airport.Id);
                context.Flights.Add(flight);
                created++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Sync complete for {Airport}: {Created} created, {Updated} updated, {Skipped} skipped",
            airport.IataCode, created, updated, skipped);

        return new SyncFlightsResponse(created, updated, skipped);
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
            return null;

        if (!DateOnly.TryParse(av.FlightDate, out var flightDate))
            return null;

        var parsed = new ParsedFlight
        {
            FlightNumber = flightNumber,
            FlightDate = flightDate,
            IataCode = av.Flight.Iata ?? string.Empty,
            IcaoCode = av.Flight.Icao ?? string.Empty,
            Airline = av.Airline.Name ?? string.Empty,
            Status = MapStatus(av.FlightStatus),
        };

        // Only attach the movement that belongs to the queried airport direction
        if (direction == MovementType.Departure)
            parsed.DepartureData = av.Departure;
        else
            parsed.ArrivalData = av.Arrival;

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
                first.IataCode = other.IataCode;
            if (!string.IsNullOrEmpty(other.IcaoCode))
                first.IcaoCode = other.IcaoCode;
            if (!string.IsNullOrEmpty(other.Airline))
                first.Airline = other.Airline;
            if (other.Status != FlightStatus.Unknown)
                first.Status = other.Status;
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
            flight.IataCode = p.IataCode;
        if (!string.IsNullOrEmpty(p.IcaoCode))
            flight.IcaoCode = p.IcaoCode;
        if (!string.IsNullOrEmpty(p.Airline))
            flight.Airline = p.Airline;
    }

    private static void AddMovement(Flight flight, ParsedFlight p, int airportId)
    {
        if (p.DepartureData?.Scheduled != null)
            flight.Movements.Add(BuildMovement(MovementType.Departure, p.DepartureData, airportId));

        if (p.ArrivalData?.Scheduled != null)
            flight.Movements.Add(BuildMovement(MovementType.Arrival, p.ArrivalData, airportId));
    }

    private static void UpsertMovement(Flight flight, ParsedFlight p, int airportId)
    {
        if (p.DepartureData?.Scheduled != null)
            UpsertSingleMovement(flight, MovementType.Departure, p.DepartureData, airportId);

        if (p.ArrivalData?.Scheduled != null)
            UpsertSingleMovement(flight, MovementType.Arrival, p.ArrivalData, airportId);
    }

    private static void UpsertSingleMovement(Flight flight, MovementType type, AviationStackMovement data, int airportId)
    {
        var existing = flight.Movements.FirstOrDefault(m => m.Type == type && m.AirportId == airportId);
        if (existing != null)
        {
            existing.ScheduledOn = data.Scheduled!.Value;
            existing.EstimatedOn = data.Estimated;
            existing.ActualOn = data.Actual;
        }
        else
        {
            flight.Movements.Add(BuildMovement(type, data, airportId));
        }
    }

    private static FlightMovement BuildMovement(MovementType type, AviationStackMovement data, int airportId)
    {
        return new FlightMovement
        {
            Type = type,
            AirportId = airportId,
            ScheduledOn = data.Scheduled!.Value,
            EstimatedOn = data.Estimated,
            ActualOn = data.Actual,
        };
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
