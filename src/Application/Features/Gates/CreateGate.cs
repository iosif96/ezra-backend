using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Gates.CreateGate;

[Authorize]
public record CreateGateCommand(int TerminalId, string Code, string PromptInformation) : IRequest<int>;

public class CreateGateCommandValidator : AbstractValidator<CreateGateCommand>
{
    public CreateGateCommandValidator()
    {
        RuleFor(x => x.TerminalId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}

internal sealed class CreateGateCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateGateCommand, int>
{
    public async Task<int> Handle(CreateGateCommand request, CancellationToken cancellationToken)
    {
        var entity = new Gate
        {
            TerminalId = request.TerminalId,
            Code = request.Code,
            PromptInformation = request.PromptInformation,
        };

        context.Gates.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
