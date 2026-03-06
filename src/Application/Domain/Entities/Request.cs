using Application.Common;
using Application.Domain.Enums;

namespace Application.Domain.Entities;

public class Request : AuditableEntity
{
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public RequestType Type { get; set; }
    public string? Content { get; set; }

    public int? StaffId { get; set; }
    public Staff? Staff { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Requested;
}
