using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.TodoItems.UpdateTodoItem;

[Authorize]
public record UpdateTodoItemCommand(int Id, string? Title, bool Done) : IRequest;

public class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator()
    {
        RuleFor(v => v.Title)
            .MaximumLength(200)
            .NotEmpty();
    }
}

internal sealed class UpdateTodoItemCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateTodoItemCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var todoItem = await _context.TodoItems
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(TodoItem), request.Id);

        todoItem.Title = request.Title;
        todoItem.Done = request.Done;

        await _context.SaveChangesAsync(cancellationToken);
    }
}