using Application.Common.Interfaces;

using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class TouchpointScanNotifier(IHubContext<TouchpointScanHub> hubContext, ILogger<TouchpointScanNotifier> logger) : ITouchpointScanNotifier
{
    public async Task NotifyScanCreated(TouchpointScanNotification notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Broadcasting ScanCreated for scan {ScanId} at touchpoint {TouchpointId}", notification.Id, notification.TouchpointId);
        await hubContext.Clients.All.SendAsync("ScanCreated", notification, cancellationToken);
        logger.LogInformation("ScanCreated broadcast complete for scan {ScanId}", notification.Id);
    }
}
