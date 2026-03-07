using Application.Common.Security;
using Application.Domain.Enums;
using Application.Features.Events.ProcessEvent;
using Application.Infrastructure.Persistence;

using FluentValidation;

using Hangfire;

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

internal sealed class CreateEventCommandHandler(
    ApplicationDbContext context,
    IBackgroundJobClient jobClient,
    IMediator mediator) : IRequestHandler<CreateEventCommand, int>
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

        if (request.ScheduledOn is not null && request.ScheduledOn > DateTime.UtcNow)
        {
            // Schedule for later
            var delay = request.ScheduledOn.Value - DateTime.UtcNow;
            jobClient.Schedule<EventJobRunner>(r => r.ProcessAsync(entity.Id), delay);
        }
        else
        {
            // Process immediately
            await mediator.Send(new ProcessEventCommand(entity.Id), cancellationToken);
        }

        return entity.Id;
    }
}
