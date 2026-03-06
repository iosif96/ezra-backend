using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateTodoItem = Application.Features.TodoItems.CreateTodoItem;
using DeleteTodoItem = Application.Features.TodoItems.DeleteTodoItem;
using GetTodoItemsWithPagination = Application.Features.TodoItems.GetTodoItemsWithPagination;
using UpdateTodoItem = Application.Features.TodoItems.UpdateTodoItem;
using UpdateTodoItemDetail = Application.Features.TodoItems.UpdateTodoItemDetail;

namespace Api.Endpoints;

[ApiController]
public class TodoItemController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTodoItem.CreateTodoItemCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTodoItem.DeleteTodoItemCommand(id));

        return NoContent();
    }

    [HttpGet]
    public Task<PaginatedList<GetTodoItemsWithPagination.TodoItemBriefResponse>> GetTodoItemsWithPagination([FromQuery] GetTodoItemsWithPagination.GetTodoItemsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTodoItem.UpdateTodoItemCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpPut("{id}/[action]")]
    public async Task<ActionResult> UpdateItemDetails(int id, UpdateTodoItemDetail.UpdateTodoItemDetailCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }
}
