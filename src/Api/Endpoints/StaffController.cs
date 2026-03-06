using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateStaff = Application.Features.Staff.CreateStaff;
using DeleteStaff = Application.Features.Staff.DeleteStaff;
using GetStaff = Application.Features.Staff.GetStaff;
using GetStaffWithPagination = Application.Features.Staff.GetStaffWithPagination;
using UpdateStaff = Application.Features.Staff.UpdateStaff;

namespace Api.Endpoints;

[ApiController]
public class StaffController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateStaff.CreateStaffCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetStaff.StaffResponse>> Get(int id)
    {
        return await Mediator.Send(new GetStaff.GetStaffQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetStaffWithPagination.StaffBriefResponse>> GetWithPagination([FromQuery] GetStaffWithPagination.GetStaffWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateStaff.UpdateStaffCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteStaff.DeleteStaffCommand(id));

        return NoContent();
    }
}
