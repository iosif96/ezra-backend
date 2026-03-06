using Application.Features.ApiKeys.CreateApiKey;
using Application.Features.ApiKeys.GetApiKeys;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

[Authorize]
public class ApiKeyController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateApiKeyCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Mediator.Send(new GetApiKeysQuery()));
    }
}