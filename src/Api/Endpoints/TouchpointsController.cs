using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateTouchpoint = Application.Features.Touchpoints.CreateTouchpoint;
using DeleteTouchpoint = Application.Features.Touchpoints.DeleteTouchpoint;
using GetTouchpoint = Application.Features.Touchpoints.GetTouchpoint;
using GetTouchpointsWithPagination = Application.Features.Touchpoints.GetTouchpointsWithPagination;
using UpdateTouchpoint = Application.Features.Touchpoints.UpdateTouchpoint;

namespace Api.Endpoints;

[ApiController]
public class TouchpointsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTouchpoint.CreateTouchpointCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetTouchpoint.TouchpointResponse>> Get(int id)
    {
        return await Mediator.Send(new GetTouchpoint.GetTouchpointQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetTouchpointsWithPagination.TouchpointBriefResponse>> GetWithPagination([FromQuery] GetTouchpointsWithPagination.GetTouchpointsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTouchpoint.UpdateTouchpointCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTouchpoint.DeleteTouchpointCommand(id));

        return NoContent();
    }
}
