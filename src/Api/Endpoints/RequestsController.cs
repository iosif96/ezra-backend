using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateRequest = Application.Features.Requests.CreateRequest;
using DeleteRequest = Application.Features.Requests.DeleteRequest;
using GetRequest = Application.Features.Requests.GetRequest;
using GetRequestsWithPagination = Application.Features.Requests.GetRequestsWithPagination;
using UpdateRequest = Application.Features.Requests.UpdateRequest;

namespace Api.Endpoints;

[ApiController]
public class RequestsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateRequest.CreateRequestCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetRequest.RequestResponse>> Get(int id)
    {
        return await Mediator.Send(new GetRequest.GetRequestQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetRequestsWithPagination.RequestBriefResponse>> GetWithPagination([FromQuery] GetRequestsWithPagination.GetRequestsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateRequest.UpdateRequestCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteRequest.DeleteRequestCommand(id));

        return NoContent();
    }
}
