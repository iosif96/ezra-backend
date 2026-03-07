using Application.Common.Interfaces;

using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class OverviewNotifier(IHubContext<OverviewHub> hubContext, ILogger<OverviewNotifier> logger) : IOverviewNotifier
{
    public async Task NotifyConversationUpdate(ConversationUpdate update, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Broadcasting ConversationUpdate");
        await hubContext.Clients.All.SendAsync("ConversationUpdate", update, cancellationToken);
    }

    public async Task NotifyRequestCreated(RequestCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Broadcasting RequestCreated for request {RequestId}", notification.Id);
        await hubContext.Clients.All.SendAsync("RequestCreated", notification, cancellationToken);
    }
}
