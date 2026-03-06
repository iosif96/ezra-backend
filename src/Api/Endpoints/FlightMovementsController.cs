using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateFlightMovement = Application.Features.FlightMovements.CreateFlightMovement;
using DeleteFlightMovement = Application.Features.FlightMovements.DeleteFlightMovement;
using GetFlightMovement = Application.Features.FlightMovements.GetFlightMovement;
using GetFlightMovementsWithPagination = Application.Features.FlightMovements.GetFlightMovementsWithPagination;
using UpdateFlightMovement = Application.Features.FlightMovements.UpdateFlightMovement;

namespace Api.Endpoints;

[ApiController]
public class FlightMovementsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateFlightMovement.CreateFlightMovementCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetFlightMovement.FlightMovementResponse>> Get(int id)
    {
        return await Mediator.Send(new GetFlightMovement.GetFlightMovementQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetFlightMovementsWithPagination.FlightMovementBriefResponse>> GetWithPagination([FromQuery] GetFlightMovementsWithPagination.GetFlightMovementsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateFlightMovement.UpdateFlightMovementCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteFlightMovement.DeleteFlightMovementCommand(id));

        return NoContent();
    }
}
