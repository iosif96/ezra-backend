using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateTerminal = Application.Features.Terminals.CreateTerminal;
using DeleteTerminal = Application.Features.Terminals.DeleteTerminal;
using GetTerminal = Application.Features.Terminals.GetTerminal;
using GetTerminalsWithPagination = Application.Features.Terminals.GetTerminalsWithPagination;
using UpdateTerminal = Application.Features.Terminals.UpdateTerminal;

namespace Api.Endpoints;

[ApiController]
public class TerminalsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTerminal.CreateTerminalCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetTerminal.TerminalResponse>> Get(int id)
    {
        return await Mediator.Send(new GetTerminal.GetTerminalQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetTerminalsWithPagination.TerminalBriefResponse>> GetWithPagination([FromQuery] GetTerminalsWithPagination.GetTerminalsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTerminal.UpdateTerminalCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTerminal.DeleteTerminalCommand(id));

        return NoContent();
    }
}
