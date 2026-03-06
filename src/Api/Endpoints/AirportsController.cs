using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateAirport = Application.Features.Airports.CreateAirport;
using DeleteAirport = Application.Features.Airports.DeleteAirport;
using GetAirport = Application.Features.Airports.GetAirport;
using GetAirportsWithPagination = Application.Features.Airports.GetAirportsWithPagination;
using UpdateAirport = Application.Features.Airports.UpdateAirport;

namespace Api.Endpoints;

[ApiController]
public class AirportsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateAirport.CreateAirportCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetAirport.AirportResponse>> Get(int id)
    {
        return await Mediator.Send(new GetAirport.GetAirportQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetAirportsWithPagination.AirportBriefResponse>> GetWithPagination([FromQuery] GetAirportsWithPagination.GetAirportsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateAirport.UpdateAirportCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteAirport.DeleteAirportCommand(id));

        return NoContent();
    }
}
