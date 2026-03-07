using Application.Domain.Enums;

namespace Application.Common.Interfaces;

public record TouchpointScanNotification(int Id, int TouchpointId, string TouchpointLabel, ChannelType ChannelType, float Latitude, float Longitude, DateTime Created);

public interface ITouchpointScanNotifier
{
    Task NotifyScanCreated(TouchpointScanNotification notification, CancellationToken cancellationToken = default);
}
