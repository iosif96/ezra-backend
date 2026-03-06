using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateIdentity = Application.Features.Identities.CreateIdentity;
using DeleteIdentity = Application.Features.Identities.DeleteIdentity;
using GetIdentitiesWithPagination = Application.Features.Identities.GetIdentitiesWithPagination;
using GetIdentity = Application.Features.Identities.GetIdentity;
using UpdateIdentity = Application.Features.Identities.UpdateIdentity;

namespace Api.Endpoints;

[ApiController]
public class IdentitiesController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateIdentity.CreateIdentityCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetIdentity.IdentityResponse>> Get(int id)
    {
        return await Mediator.Send(new GetIdentity.GetIdentityQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetIdentitiesWithPagination.IdentityBriefResponse>> GetWithPagination([FromQuery] GetIdentitiesWithPagination.GetIdentitiesWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateIdentity.UpdateIdentityCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteIdentity.DeleteIdentityCommand(id));

        return NoContent();
    }
}
