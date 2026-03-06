using Application.Common.Interfaces.BlobStorage;
using Application.Common.Models.BlobStorage;

using Microsoft.AspNetCore.Http;

namespace Application.Features.FileUpload.UploadFile;

public record UploadFileCommand(IFormFile File) : IRequest<string>;

internal sealed class UploadFileHandler(IConcreteStorageClient concreteStorageClient) : IRequestHandler<UploadFileCommand, string>
{
    private readonly IConcreteStorageClient _concreteStorageClient = concreteStorageClient;

    public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        InputBlob inputBlob = new InputBlob
        {
            BlobName = request.File.FileName,
            ContentType = request.File.ContentType,
            UploadType = UploadType.Stream,
            StreamFile = request.File.OpenReadStream(),
            Metadata = new Dictionary<string, string>(),
        };

        var file = await _concreteStorageClient.UploadAsync(inputBlob, cancellationToken);

        return file.Uri.AbsoluteUri;
    }
}

