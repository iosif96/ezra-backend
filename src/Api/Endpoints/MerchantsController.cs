using Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

using CreateMerchant = Application.Features.Merchants.CreateMerchant;
using DeleteMerchant = Application.Features.Merchants.DeleteMerchant;
using GetMerchant = Application.Features.Merchants.GetMerchant;
using GetMerchantsWithPagination = Application.Features.Merchants.GetMerchantsWithPagination;
using UpdateMerchant = Application.Features.Merchants.UpdateMerchant;

namespace Api.Endpoints;

[ApiController]
public class MerchantsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateMerchant.CreateMerchantCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetMerchant.MerchantResponse>> Get(int id)
    {
        return await Mediator.Send(new GetMerchant.GetMerchantQuery(id));
    }

    [HttpGet]
    public Task<PaginatedList<GetMerchantsWithPagination.MerchantBriefResponse>> GetWithPagination([FromQuery] GetMerchantsWithPagination.GetMerchantsWithPaginationQuery query)
    {
        return Mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateMerchant.UpdateMerchantCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteMerchant.DeleteMerchantCommand(id));

        return NoContent();
    }
}
