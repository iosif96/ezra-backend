using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateConversation = Application.Features.Conversations.CreateConversation;
using DeleteConversation = Application.Features.Conversations.DeleteConversation;
using GetConversation = Application.Features.Conversations.GetConversation;
using GetConversationsWithPagination = Application.Features.Conversations.GetConversationsWithPagination;
using ResetConversation = Application.Features.Conversations.ResetConversation;
using UpdateConversation = Application.Features.Conversations.UpdateConversation;

namespace Api.Endpoints;

[ApiController]
public class ConversationsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateConversation.CreateConversationCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetConversation.ConversationResponse>> Get(int id)
    {
        return await Mediator.Send(new GetConversation.GetConversationQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetConversationsWithPagination.ConversationBriefResponse>> GetWithPagination([FromQuery] GetConversationsWithPagination.GetConversationsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateConversation.UpdateConversationCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpPost("{id}/reset")]
    public async Task<ActionResult> Reset(int id)
    {
        await Mediator.Send(new ResetConversation.ResetConversationCommand(id));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteConversation.DeleteConversationCommand(id));

        return NoContent();
    }
}
