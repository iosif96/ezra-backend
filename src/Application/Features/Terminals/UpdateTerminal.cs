using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Terminals.UpdateTerminal;

[Authorize]
public record UpdateTerminalCommand(int Id, string Code, string PromptInformation) : IRequest;

public class UpdateTerminalCommandValidator : AbstractValidator<UpdateTerminalCommand>
{
    public UpdateTerminalCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}

internal sealed class UpdateTerminalCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateTerminalCommand>
{
    public async Task Handle(UpdateTerminalCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Terminals
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Terminal), request.Id);

        entity.Code = request.Code;
        entity.PromptInformation = request.PromptInformation;

        await context.SaveChangesAsync(cancellationToken);
    }
}
