using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Events.CreateEvent;

[Authorize]
public record CreateEventCommand(EventType Type, int? FlightId, DateTime? ScheduledOn, string Content) : IRequest<int>;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}

internal sealed class CreateEventCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateEventCommand, int>
{
    public async Task<int> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = new Application.Domain.Entities.Event
        {
            Type = request.Type,
            FlightId = request.FlightId,
            ScheduledOn = request.ScheduledOn,
            Content = request.Content,
        };

        context.Events.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
