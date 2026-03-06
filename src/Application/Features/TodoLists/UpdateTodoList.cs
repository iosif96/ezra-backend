using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.TodoLists.UpdateTodoList;

[Authorize]
public class UpdateTodoListCommand : IRequest
{
    public int Id { get; set; }

    public string? Title { get; set; }
}

public class UpdateTodoListCommandValidator : AbstractValidator<UpdateTodoListCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateTodoListCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .Must((model, title) => BeUniqueTitle(model, title)).WithMessage("The specified title already exists.");
    }

    public bool BeUniqueTitle(UpdateTodoListCommand model, string? title)
    {
        return !_context.TodoLists
            .Where(l => l.Id != model.Id)
            .Any(l => l.Title == title);
    }
}

internal sealed class UpdateTodoListCommandHandler : IRequestHandler<UpdateTodoListCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateTodoListCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTodoListCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoLists
            .FindAsync(new object[] { request.Id }, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException(nameof(TodoList), request.Id);

        entity.Title = request.Title;

        await _context.SaveChangesAsync(cancellationToken);
    }
}