using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.TodoLists.CreateTodoList;

[Authorize]
public record CreateTodoListCommand(string? Title) : IRequest<int>;

public class CreateTodoListCommandValidator : AbstractValidator<CreateTodoListCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateTodoListCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .Must(BeUniqueTitle).WithMessage("The specified title already exists.");
    }

    private bool BeUniqueTitle(string? title)
    {
        return !_context.TodoLists
            .Any(l => l.Title == title);
    }
}

internal sealed class CreateTodoListCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateTodoListCommand, int>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<int> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        var todoList = new TodoList { Title = request.Title };

        _context.TodoLists.Add(todoList);

        await _context.SaveChangesAsync(cancellationToken);

        return todoList.Id;
    }
}