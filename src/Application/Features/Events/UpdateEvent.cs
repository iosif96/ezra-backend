using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Events.UpdateEvent;

[Authorize]
public record UpdateEventCommand(int Id, DateTime? ScheduledOn, string Content) : IRequest;

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}

internal sealed class UpdateEventCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateEventCommand>
{
    public async Task Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Events
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Application.Domain.Entities.Event), request.Id);

        entity.ScheduledOn = request.ScheduledOn;
        entity.Content = request.Content;

        await context.SaveChangesAsync(cancellationToken);
    }
}
