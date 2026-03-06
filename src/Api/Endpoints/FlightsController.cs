using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateFlight = Application.Features.Flights.CreateFlight;
using DeleteFlight = Application.Features.Flights.DeleteFlight;
using GetFlight = Application.Features.Flights.GetFlight;
using GetFlightsWithPagination = Application.Features.Flights.GetFlightsWithPagination;
using SyncFlights = Application.Features.Flights.SyncFlights;
using UpdateFlight = Application.Features.Flights.UpdateFlight;

namespace Api.Endpoints;

[ApiController]
public class FlightsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateFlight.CreateFlightCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetFlight.FlightResponse>> Get(int id)
    {
        return await Mediator.Send(new GetFlight.GetFlightQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetFlightsWithPagination.FlightBriefResponse>> GetWithPagination([FromQuery] GetFlightsWithPagination.GetFlightsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateFlight.UpdateFlightCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteFlight.DeleteFlightCommand(id));

        return NoContent();
    }

    [HttpPost("sync/{airportId}")]
    public async Task<ActionResult<SyncFlights.SyncFlightsResponse>> Sync(int airportId)
    {
        return await Mediator.Send(new SyncFlights.SyncFlightsCommand(airportId));
    }
}
