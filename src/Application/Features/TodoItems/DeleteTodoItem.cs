using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Events;
using Application.Infrastructure.Persistence;

namespace Application.Features.TodoItems.DeleteTodoItem;

[Authorize]
public record DeleteTodoItemCommand(int Id) : IRequest;

internal sealed class DeleteTodoItemCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(TodoItem), request.Id);
        _context.TodoItems.Remove(entity);

        entity.AddDomainEvent(new TodoItemDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);
    }
}