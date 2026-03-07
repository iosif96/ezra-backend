using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateTouchpointScan = Application.Features.TouchpointScans.CreateTouchpointScan;
using DeleteTouchpointScan = Application.Features.TouchpointScans.DeleteTouchpointScan;
using GetTouchpointScan = Application.Features.TouchpointScans.GetTouchpointScan;
using GetTouchpointScansWithPagination = Application.Features.TouchpointScans.GetTouchpointScansWithPagination;
using UpdateTouchpointScan = Application.Features.TouchpointScans.UpdateTouchpointScan;

namespace Api.Endpoints;

[ApiController]
public class TouchpointScansController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateTouchpointScan.CreateTouchpointScanResponse>> Create(CreateTouchpointScan.CreateTouchpointScanCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetTouchpointScan.TouchpointScanResponse>> Get(int id)
    {
        return await Mediator.Send(new GetTouchpointScan.GetTouchpointScanQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetTouchpointScansWithPagination.TouchpointScanBriefResponse>> GetWithPagination([FromQuery] GetTouchpointScansWithPagination.GetTouchpointScansWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTouchpointScan.UpdateTouchpointScanCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTouchpointScan.DeleteTouchpointScanCommand(id));

        return NoContent();
    }
}
