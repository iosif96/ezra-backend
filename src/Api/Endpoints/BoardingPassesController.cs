using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateBoardingPass = Application.Features.BoardingPasses.CreateBoardingPass;
using DeleteBoardingPass = Application.Features.BoardingPasses.DeleteBoardingPass;
using GetBoardingPass = Application.Features.BoardingPasses.GetBoardingPass;
using GetBoardingPassesWithPagination = Application.Features.BoardingPasses.GetBoardingPassesWithPagination;
using UpdateBoardingPass = Application.Features.BoardingPasses.UpdateBoardingPass;

namespace Api.Endpoints;

[ApiController]
public class BoardingPassesController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateBoardingPass.CreateBoardingPassCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetBoardingPass.BoardingPassResponse>> Get(int id)
    {
        return await Mediator.Send(new GetBoardingPass.GetBoardingPassQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetBoardingPassesWithPagination.BoardingPassBriefResponse>> GetWithPagination([FromQuery] GetBoardingPassesWithPagination.GetBoardingPassesWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateBoardingPass.UpdateBoardingPassCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteBoardingPass.DeleteBoardingPassCommand(id));

        return NoContent();
    }
}
