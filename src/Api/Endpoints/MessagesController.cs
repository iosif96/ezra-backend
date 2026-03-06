using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateMessage = Application.Features.Messages.CreateMessage;
using DeleteMessage = Application.Features.Messages.DeleteMessage;
using GetMessage = Application.Features.Messages.GetMessage;
using GetMessagesWithPagination = Application.Features.Messages.GetMessagesWithPagination;
using UpdateMessage = Application.Features.Messages.UpdateMessage;

namespace Api.Endpoints;

[ApiController]
public class MessagesController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateMessage.CreateMessageCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetMessage.MessageResponse>> Get(int id)
    {
        return await Mediator.Send(new GetMessage.GetMessageQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetMessagesWithPagination.MessageBriefResponse>> GetWithPagination([FromQuery] GetMessagesWithPagination.GetMessagesWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateMessage.UpdateMessageCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteMessage.DeleteMessageCommand(id));

        return NoContent();
    }
}
