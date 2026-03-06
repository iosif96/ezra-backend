using Application.Common;

namespace Application.Domain.Entities;

public class Feedback : AuditableEntity
{
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;

    public int Rating { get; set; }
    public string? Content { get; set; }
    public float? SentimentScore { get; set; }
}
