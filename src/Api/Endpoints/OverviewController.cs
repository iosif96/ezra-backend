using Microsoft.AspNetCore.Mvc;

using GetOverview = Application.Features.Overview.GetOverview;

namespace Api.Endpoints;

[ApiController]
public class OverviewController : ApiControllerBase
{
    [HttpGet]
    public async Task<GetOverview.OverviewResponse> Get()
    {
        return await Mediator.Send(new GetOverview.GetOverviewQuery());
    }
}
