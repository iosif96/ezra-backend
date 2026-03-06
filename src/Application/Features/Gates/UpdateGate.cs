using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Gates.UpdateGate;

[Authorize]
public record UpdateGateCommand(int Id, string Code, string PromptInformation) : IRequest;

public class UpdateGateCommandValidator : AbstractValidator<UpdateGateCommand>
{
    public UpdateGateCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}

internal sealed class UpdateGateCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateGateCommand>
{
    public async Task Handle(UpdateGateCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Gates
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Gate), request.Id);

        entity.Code = request.Code;
        entity.PromptInformation = request.PromptInformation;

        await context.SaveChangesAsync(cancellationToken);
    }
}
