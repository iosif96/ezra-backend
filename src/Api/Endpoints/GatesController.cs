using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateGate = Application.Features.Gates.CreateGate;
using DeleteGate = Application.Features.Gates.DeleteGate;
using GetGate = Application.Features.Gates.GetGate;
using GetGatesWithPagination = Application.Features.Gates.GetGatesWithPagination;
using UpdateGate = Application.Features.Gates.UpdateGate;

namespace Api.Endpoints;

[ApiController]
public class GatesController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateGate.CreateGateCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetGate.GateResponse>> Get(int id)
    {
        return await Mediator.Send(new GetGate.GetGateQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetGatesWithPagination.GateBriefResponse>> GetWithPagination([FromQuery] GetGatesWithPagination.GetGatesWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateGate.UpdateGateCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteGate.DeleteGateCommand(id));

        return NoContent();
    }
}
