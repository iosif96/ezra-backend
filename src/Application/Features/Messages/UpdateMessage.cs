using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Messages.UpdateMessage;

[Authorize]
public record UpdateMessageCommand(int Id, string? Content, float? StressIndex) : IRequest;

public class UpdateMessageCommandValidator : AbstractValidator<UpdateMessageCommand>
{
    public UpdateMessageCommandValidator()
    {
        RuleFor(x => x.StressIndex).InclusiveBetween(0f, 1f).When(x => x.StressIndex.HasValue);
    }
}

internal sealed class UpdateMessageCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateMessageCommand>
{
    public async Task Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Messages
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Message), request.Id);

        entity.Content = request.Content;
        entity.StressIndex = request.StressIndex;

        await context.SaveChangesAsync(cancellationToken);
    }
}
