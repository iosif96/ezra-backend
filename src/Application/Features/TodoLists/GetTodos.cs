using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.TodoLists.GetTodos;

[Authorize]
[AllowApiKey]
public record GetTodosQuery : IRequest<TodosVm>;

public class TodosVm
{
    public IList<PriorityLevelDto> PriorityLevels { get; set; } = new List<PriorityLevelDto>();

    public IList<TodoListDto> Lists { get; set; } = new List<TodoListDto>();
}

public record PriorityLevelDto(int Value, string? Name);

public record TodoListDto(int Id, string? Title, IList<TodoItemDto> Items)
{
    public TodoListDto()
        : this(default, null, new List<TodoItemDto>())
    {
    }
}

public record TodoItemDto(int Id, int ListId, string? Title, bool Done, int Priority, string? Note);

internal sealed class GetTodosQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTodosQuery, TodosVm>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        return new TodosVm
        {
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                .Cast<PriorityLevel>()
                .Select(p => new PriorityLevelDto((int)p, p.ToString()))
                .ToList(),

            Lists = await _context.TodoLists
                .Include(t => t.Items)
                .AsNoTracking()
                .OrderBy(t => t.Title)
                .Select(todoListItem => ToDto(todoListItem))
                .ToListAsync(cancellationToken),
        };
    }

    private static TodoItemDto ToDto(TodoItem todoItem)
    {
        var todoItemDto = new TodoItemDto(todoItem.Id, todoItem.ListId, todoItem.Title, todoItem.Done, (int)todoItem.Priority, todoItem.Note);

        return todoItemDto;
    }

    private static TodoListDto ToDto(TodoList todoList)
    {
        var todoListDto = new TodoListDto
        {
            Id = todoList.Id,
            Title = todoList.Title,
            Items = todoList.Items
                .Select(item => ToDto(item))
                .ToList(),
        };

        return todoListDto;
    }
}