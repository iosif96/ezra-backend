using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UploadFile = Application.Features.FileUpload.UploadFile;

namespace Api.Endpoints;

[ApiController]
[Authorize]
public class UploadFilesController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<string>> UploadFile(UploadFile.UploadFileCommand command)
    {
        return await Mediator.Send(command);
    }
}
