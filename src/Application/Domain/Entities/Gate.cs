using Application.Common;

namespace Application.Domain.Entities;

public class Gate : AuditableEntity
{
    public int TerminalId { get; set; }
    public Terminal Terminal { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public string PromptInformation { get; set; } = string.Empty;
}
