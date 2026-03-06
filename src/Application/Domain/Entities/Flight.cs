using Application.Common;
using Application.Domain.Enums;

namespace Application.Domain.Entities;

public class Flight : AuditableEntity
{
    public DateOnly Date { get; set; }
    public string Number { get; set; } = string.Empty;
    public string IataCode { get; set; } = string.Empty;
    public string IcaoCode { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;
    public FlightSource FlightSource { get; set; }

    public ICollection<FlightMovement> Movements { get; set; } = new List<FlightMovement>();
    public ICollection<BoardingPass> BoardingPasses { get; set; } = new List<BoardingPass>();
}
