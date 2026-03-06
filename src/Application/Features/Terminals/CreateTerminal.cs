using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Terminals.CreateTerminal;

[Authorize]
public record CreateTerminalCommand(int AirportId, string Code, string PromptInformation) : IRequest<int>;

public class CreateTerminalCommandValidator : AbstractValidator<CreateTerminalCommand>
{
    public CreateTerminalCommandValidator()
    {
        RuleFor(x => x.AirportId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}

internal sealed class CreateTerminalCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateTerminalCommand, int>
{
    public async Task<int> Handle(CreateTerminalCommand request, CancellationToken cancellationToken)
    {
        var entity = new Terminal
        {
            AirportId = request.AirportId,
            Code = request.Code,
            PromptInformation = request.PromptInformation,
        };

        context.Terminals.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
