using Application.Common;

namespace Application.Domain.Entities;

public class Merchant : AuditableEntity
{
    public int TerminalId { get; set; }
    public Terminal Terminal { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public bool IsAirside { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string PromptInformation { get; set; } = string.Empty;
}
