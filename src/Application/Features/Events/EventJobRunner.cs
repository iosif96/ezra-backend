using Application.Features.Events.ProcessEvent;

namespace Application.Features.Events;

public class EventJobRunner(IMediator mediator)
{
    public async Task ProcessAsync(int eventId)
    {
        await mediator.Send(new ProcessEventCommand(eventId));
    }
}
