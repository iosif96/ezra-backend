using Application.Common;
using Application.Domain.Enums;

namespace Application.Domain.Entities;

public class TouchpointScan : AuditableEntity
{
    public int TouchpointId { get; set; }
    public Touchpoint Touchpoint { get; set; } = null!;

    public ChannelType ChannelType { get; set; }
}
