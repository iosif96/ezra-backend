using Application.Common;

namespace Application.Domain.Entities;

public class Touchpoint : AuditableEntity
{
    public int AirportId { get; set; }
    public Airport Airport { get; set; } = null!;

    public int? TerminalId { get; set; }
    public Terminal? Terminal { get; set; }

    public int? GateId { get; set; }
    public Gate? Gate { get; set; }

    public string Label { get; set; } = string.Empty;
    public float Latitude { get; set; }
    public float Longitude { get; set; }

    public ICollection<TouchpointScan> Scans { get; set; } = new List<TouchpointScan>();
}
