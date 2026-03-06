using Application.Features.TodoLists;

using Microsoft.AspNetCore.Mvc;

using CreateTodoList = Application.Features.TodoLists.CreateTodoList;
using DeleteTodoList = Application.Features.TodoLists.DeleteTodoList;
using ExportTodos = Application.Features.TodoLists.ExportTodos;

using GetTodos = Application.Features.TodoLists.GetTodos;
using UpdateTodoList = Application.Features.TodoLists.UpdateTodoList;
namespace Api.Endpoints;

[ApiController]
public class TodoListController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTodoList.CreateTodoListCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteTodoList.DeleteTodoListCommand(id));

        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<FileResult> Get(int id)
    {
        var vm = await Mediator.Send(new ExportTodos.ExportTodosQuery(id));

        return File(vm.Content, vm.ContentType, vm.FileName);
    }

    [HttpGet]
    public async Task<ActionResult<GetTodos.TodosVm>> Get()
    {
        return await Mediator.Send(new GetTodos.GetTodosQuery());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTodoList.UpdateTodoListCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }
}
