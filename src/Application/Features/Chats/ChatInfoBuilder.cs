using Application.Domain.Entities;

namespace Application.Features.Chats;

public static class ChatInfoBuilder
{
    public static string BuildAirportInfo(Airport airport)
    {
        var info = $"{airport.Name} ({airport.IataCode}), {airport.City}, {airport.CountryCode}";

        if (!string.IsNullOrWhiteSpace(airport.PromptInformation))
            info += $"\n{airport.PromptInformation}";

        foreach (var terminal in airport.Terminals)
        {
            if (!string.IsNullOrWhiteSpace(terminal.PromptInformation))
                info += $"\nTerminal {terminal.Code}: {terminal.PromptInformation}";

            foreach (var gate in terminal.Gates.Where(g => !string.IsNullOrWhiteSpace(g.PromptInformation)))
                info += $"\nGate {gate.Code}: {gate.PromptInformation}";

            foreach (var merchant in terminal.Merchants.Where(m => !string.IsNullOrWhiteSpace(m.PromptInformation)))
                info += $"\nMerchant '{merchant.Name}' (Terminal {terminal.Code}, {(merchant.IsAirside ? "airside" : "landside")}): {merchant.PromptInformation}";
        }

        return info;
    }

    public static string BuildFlightInfo(Flight flight)
    {
        var info = $"Flight: {flight.Number} ({flight.Airline}), Date: {flight.Date:yyyy-MM-dd}, Status: {flight.Status}";

        foreach (var movement in flight.Movements.OrderBy(m => m.ScheduledOn))
        {
            info += $"\n{movement.Type} ({movement.Airport.IataCode}): Scheduled {movement.ScheduledOn:yyyy-MM-dd HH:mm}";
            if (movement.EstimatedOn is not null)
                info += $", Estimated {movement.EstimatedOn:HH:mm}";
            if (movement.ActualOn is not null)
                info += $", Actual {movement.ActualOn:HH:mm}";
            if (movement.BoardingOn is not null)
                info += $", Boarding {movement.BoardingOn:HH:mm}";
            if (movement.GateCloseOn is not null)
                info += $", Gate closes {movement.GateCloseOn:HH:mm}";
            if (movement.Terminal is not null)
                info += $", Terminal: {movement.Terminal.Code}";
            if (movement.Gate is not null)
                info += $", Gate: {movement.Gate.Code}";
        }

        return info;
    }
}
