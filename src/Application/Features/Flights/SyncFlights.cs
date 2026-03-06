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
            .FirstOrDefaultAsync(a => a.Id == request.AirportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), request.AirportId);

        var today = DateOnly.FromDateTime(dateTime.Now);
        var tomorrow = today.AddDays(1);

        var todayFlights = await aviationStack.GetFlightsAsync(airport.IataCode, today, cancellationToken);
        var tomorrowFlights = await aviationStack.GetFlightsAsync(airport.IataCode, tomorrow, cancellationToken);

        var allFlights = todayFlights.Concat(tomorrowFlights).ToList();

        logger.LogInformation("Syncing {Count} flights for airport {Airport}", allFlights.Count, airport.IataCode);

        var created = 0;
        var updated = 0;

        foreach (var avFlight in allFlights)
        {
            var flightNumber = avFlight.Flight.Iata ?? avFlight.Flight.Icao ?? avFlight.Flight.Number;
            if (string.IsNullOrWhiteSpace(flightNumber))
                continue;

            if (!DateOnly.TryParse(avFlight.FlightDate, out var flightDate))
                continue;

            var existing = await context.Flights
                .Include(f => f.Movements)
                .FirstOrDefaultAsync(f => f.Number == flightNumber && f.Date == flightDate, cancellationToken);

            if (existing == null)
            {
                var flight = CreateFlight(avFlight, flightNumber, flightDate);
                context.Flights.Add(flight);

                AddMovements(flight, avFlight, airport.Id);
                created++;
            }
            else
            {
                UpdateFlight(existing, avFlight);
                UpdateMovements(existing, avFlight, airport.Id);
                updated++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Sync complete for {Airport}: {Created} created, {Updated} updated", airport.IataCode, created, updated);

        return new SyncFlightsResponse(created, updated);
    }

    private static Flight CreateFlight(AviationStackFlight av, string flightNumber, DateOnly flightDate)
    {
        return new Flight
        {
            Date = flightDate,
            Number = flightNumber,
            IataCode = av.Flight.Iata ?? string.Empty,
            IcaoCode = av.Flight.Icao ?? string.Empty,
            Airline = av.Airline.Name ?? string.Empty,
            Status = MapStatus(av.FlightStatus),
            FlightSource = FlightSource.AviationStack,
        };
    }

    private static void UpdateFlight(Flight flight, AviationStackFlight av)
    {
        flight.Status = MapStatus(av.FlightStatus);

        if (!string.IsNullOrEmpty(av.Flight.Iata))
            flight.IataCode = av.Flight.Iata;
        if (!string.IsNullOrEmpty(av.Flight.Icao))
            flight.IcaoCode = av.Flight.Icao;
        if (!string.IsNullOrEmpty(av.Airline.Name))
            flight.Airline = av.Airline.Name;
    }

    private static void AddMovements(Flight flight, AviationStackFlight av, int airportId)
    {
        if (av.Departure.Scheduled.HasValue)
        {
            flight.Movements.Add(BuildMovement(MovementType.Departure, av.Departure, airportId));
        }

        if (av.Arrival.Scheduled.HasValue)
        {
            flight.Movements.Add(BuildMovement(MovementType.Arrival, av.Arrival, airportId));
        }
    }

    private static void UpdateMovements(Flight flight, AviationStackFlight av, int airportId)
    {
        UpdateOrAddMovement(flight, MovementType.Departure, av.Departure, airportId);
        UpdateOrAddMovement(flight, MovementType.Arrival, av.Arrival, airportId);
    }

    private static void UpdateOrAddMovement(Flight flight, MovementType type, AviationStackMovement av, int airportId)
    {
        if (!av.Scheduled.HasValue)
            return;

        var movement = flight.Movements.FirstOrDefault(m => m.Type == type);
        if (movement != null)
        {
            movement.ScheduledOn = av.Scheduled.Value;
            movement.EstimatedOn = av.Estimated;
            movement.ActualOn = av.Actual;
        }
        else
        {
            flight.Movements.Add(BuildMovement(type, av, airportId));
        }
    }

    private static FlightMovement BuildMovement(MovementType type, AviationStackMovement av, int airportId)
    {
        return new FlightMovement
        {
            Type = type,
            AirportId = airportId,
            ScheduledOn = av.Scheduled!.Value,
            EstimatedOn = av.Estimated,
            ActualOn = av.Actual,
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
