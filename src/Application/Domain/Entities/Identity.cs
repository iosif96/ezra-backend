using Application.Common;

namespace Application.Domain.Entities;

public class Identity : AuditableEntity
{
    public string? PassengerName { get; set; }

    public ICollection<BoardingPass> BoardingPasses { get; set; } = new List<BoardingPass>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
