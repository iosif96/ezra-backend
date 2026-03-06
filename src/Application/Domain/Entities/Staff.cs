using Application.Common;

namespace Application.Domain.Entities;

public class Staff : AuditableEntity
{
    public int AirportId { get; set; }
    public Airport Airport { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
