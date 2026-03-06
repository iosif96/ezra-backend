using Application.Common;

namespace Application.Domain.Entities;

public class Terminal : AuditableEntity
{
    public int AirportId { get; set; }
    public Airport Airport { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string PromptInformation { get; set; } = string.Empty;

    public ICollection<Gate> Gates { get; set; } = new List<Gate>();
    public ICollection<Merchant> Merchants { get; set; } = new List<Merchant>();
}
