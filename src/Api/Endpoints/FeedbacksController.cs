using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateFeedback = Application.Features.Feedbacks.CreateFeedback;
using DeleteFeedback = Application.Features.Feedbacks.DeleteFeedback;
using GetFeedback = Application.Features.Feedbacks.GetFeedback;
using GetFeedbacksWithPagination = Application.Features.Feedbacks.GetFeedbacksWithPagination;
using UpdateFeedback = Application.Features.Feedbacks.UpdateFeedback;

namespace Api.Endpoints;

[ApiController]
public class FeedbacksController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateFeedback.CreateFeedbackCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetFeedback.FeedbackResponse>> Get(int id)
    {
        return await Mediator.Send(new GetFeedback.GetFeedbackQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetFeedbacksWithPagination.FeedbackBriefResponse>> GetWithPagination([FromQuery] GetFeedbacksWithPagination.GetFeedbacksWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateFeedback.UpdateFeedbackCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteFeedback.DeleteFeedbackCommand(id));

        return NoContent();
    }
}
