using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateEvent = Application.Features.Events.CreateEvent;
using DeleteEvent = Application.Features.Events.DeleteEvent;
using GetEvent = Application.Features.Events.GetEvent;
using GetEventsWithPagination = Application.Features.Events.GetEventsWithPagination;
using UpdateEvent = Application.Features.Events.UpdateEvent;

namespace Api.Endpoints;

[ApiController]
public class EventsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateEvent.CreateEventCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetEvent.EventResponse>> Get(int id)
    {
        return await Mediator.Send(new GetEvent.GetEventQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetEventsWithPagination.EventBriefResponse>> GetWithPagination([FromQuery] GetEventsWithPagination.GetEventsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateEvent.UpdateEventCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteEvent.DeleteEventCommand(id));

        return NoContent();
    }
}
