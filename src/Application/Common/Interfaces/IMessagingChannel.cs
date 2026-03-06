namespace Application.Common.Interfaces;

public interface IMessagingChannel
{
    Task SendTextMessageAsync(string channelId, string text, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadMediaAsync(string mediaId, CancellationToken cancellationToken = default);
}
