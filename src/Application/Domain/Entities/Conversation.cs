using Application.Common;
using Application.Domain.Enums;

namespace Application.Domain.Entities;

public class Conversation : AuditableEntity
{
    public ChannelType ChannelType { get; set; }
    public string ChannelId { get; set; } = string.Empty;

    public int? IdentityId { get; set; }
    public Identity? Identity { get; set; }

    public DateTime LastMessageOn { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
