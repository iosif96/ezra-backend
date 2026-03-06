using Application.Common;
using Application.Domain.Enums;

namespace Application.Domain.Entities;

public class Message : AuditableEntity
{
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public MessageType Type { get; set; }
    public MediaType MediaType { get; set; }
    public string? MediaUrl { get; set; }
    public string? Content { get; set; }
    public string? Model { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public float? StressIndex { get; set; }
}
