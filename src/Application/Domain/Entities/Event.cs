using Application.Common;
using Application.Domain.Enums;

namespace Application.Domain.Entities;

public class Event : AuditableEntity
{
    public EventType Type { get; set; }

    public int? FlightId { get; set; }
    public Flight? Flight { get; set; }

    public DateTime? ScheduledOn { get; set; }
    public string Content { get; set; } = string.Empty;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
