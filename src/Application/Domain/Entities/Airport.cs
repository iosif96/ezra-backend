using Application.Common;

namespace Application.Domain.Entities;

public class Airport : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string IataCode { get; set; } = string.Empty;
    public string IcaoCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = string.Empty;
    public string PromptInformation { get; set; } = string.Empty;

    public ICollection<Terminal> Terminals { get; set; } = new List<Terminal>();
    public ICollection<Staff> Staff { get; set; } = new List<Staff>();
    public ICollection<Touchpoint> Touchpoints { get; set; } = new List<Touchpoint>();
}
