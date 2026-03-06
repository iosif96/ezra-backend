using Application.Common;
using Application.Domain.Enums;

namespace Application.Domain.Entities;

public class FlightMovement : AuditableEntity
{
    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;

    public MovementType Type { get; set; }

    public int AirportId { get; set; }
    public Airport Airport { get; set; } = null!;

    public int? TerminalId { get; set; }
    public Terminal? Terminal { get; set; }

    public int? GateId { get; set; }
    public Gate? Gate { get; set; }

    public DateTime? BoardingOn { get; set; }
    public DateTime? GateCloseOn { get; set; }
    public DateTime ScheduledOn { get; set; }
    public DateTime? EstimatedOn { get; set; }
    public DateTime? ActualOn { get; set; }
}
