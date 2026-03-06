using Application.Common;

namespace Application.Domain.Entities;

public class BoardingPass : AuditableEntity
{
    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string? Seat { get; set; }

    public int IdentityId { get; set; }
    public Identity Identity { get; set; } = null!;
}
