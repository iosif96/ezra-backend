namespace Application.Domain.Entities;

public class TodoList : AuditableEntity
{
    public string? Title { get; set; }

    public IList<TodoItem> Items { get; private set; } = new List<TodoItem>();
}