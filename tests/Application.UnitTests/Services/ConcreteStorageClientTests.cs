using Application.Common.Models.BlobStorage;
using Application.Infrastructure.Services.BlobStorage;

using Azure.Storage.Blobs;

using Microsoft.Extensions.Options;

using Moq;

namespace Application.UnitTests.Services;

public class ConcreteStorageClientTests
{
    private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
    private readonly Mock<BlobClient> _mockBlobClient;
    private readonly ConcreteStorageClient _storageClient;

    public ConcreteStorageClientTests()
    {
        // Mock dependencies
        _mockBlobContainerClient = new Mock<BlobContainerClient>();
        _mockBlobClient = new Mock<BlobClient>();

        _mockBlobContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);

        // Mock options for AzureStorageOptions. Add real credentials here
        var mockOptions = new Mock<IOptions<AzureStorageOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new AzureStorageOptions
        {
            ConnectionString = "YOUR_CONTAINER_URL",
            ContainerName = "test-container",
        });

        // Create instance of ConcreteStorageClient using mock options and mock blob container client
        _storageClient = new ConcreteStorageClient(mockOptions.Object);
    }

    [Fact]
    public async Task UploadAsync_UploadsBinaryBlob_ReturnsFileStorage()
    {
        var inputBlob = new InputBlob
        {
            BlobName = "test-blob",
            ContentType = "application/octet-stream",
            UploadType = UploadType.Stream,
            StreamFile = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 }),
            Metadata = new Dictionary<string, string> { { "key", "value" } },
            Tags = new Dictionary<string, string> { { "tagKey", "tagValue" } },
        };
        var result = await _storageClient.UploadAsync(inputBlob, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("test-blob", result.BlobName);
        Assert.Equal(new Uri("https://gsconstructiialex.blob.core.windows.net/test-container/test-blob"), result.Uri);
        var fileExists = await _storageClient.FileExistsAsync("test-blob");
        Assert.True(fileExists);

        // Cleanup
        await _storageClient.DeleteFileAsync(result.BlobName, CancellationToken.None);
    }

    [Fact]
    public async Task DeleteFileAsync_DeletesBlobSuccessfully()
    {
        // Setup for deletion
        var blobName = "test-blob";
        var inputBlob = new InputBlob
        {
            BlobName = blobName,
            ContentType = "application/octet-stream",
            UploadType = UploadType.Stream,
            StreamFile = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 }),
            Metadata = new Dictionary<string, string> { { "key", "value" } },
            Tags = new Dictionary<string, string> { { "tagKey", "tagValue" } },
        };

        // Mock the DeleteBlobAsync method
        await _storageClient.UploadAsync(inputBlob, CancellationToken.None);
        var fileExists = await _storageClient.FileExistsAsync(blobName);
        Assert.True(fileExists);

        // Act
        await _storageClient.DeleteFileAsync(blobName, CancellationToken.None);
        fileExists = await _storageClient.FileExistsAsync(blobName);
        Assert.False(fileExists);
    }

    [Fact]
    public async Task DownloadLargeFile_ReturnsStream()
    {
        var blobName = "test-blob";
        var inputBlob = new InputBlob
        {
            BlobName = blobName,
            ContentType = "application/octet-stream",
            UploadType = UploadType.Stream,
            StreamFile = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 }),
            Metadata = new Dictionary<string, string> { { "key", "value" } },
            Tags = new Dictionary<string, string> { { "tagKey", "tagValue" } },
        };
        await _storageClient.UploadAsync(inputBlob, CancellationToken.None);
        var stream = await _storageClient.DownloadLargeBlobAsync(blobName, CancellationToken.None);

        Assert.NotNull(stream);
        Assert.IsAssignableFrom<Stream>(stream);
        await _storageClient.DeleteFileAsync(blobName, CancellationToken.None);
    }
}
