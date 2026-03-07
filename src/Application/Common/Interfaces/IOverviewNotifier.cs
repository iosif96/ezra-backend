using Application.Domain.Enums;

namespace Application.Common.Interfaces;

public record ConversationUpdate(
    int MessagesLastHour,
    int ConversationsLastHour,
    int ConversationsToday);

public record RequestCreatedNotification(
    int Id,
    RequestType Type,
    string? Content,
    RequestStatus Status,
    ChannelType ChannelType,
    DateTime Created);

public interface IOverviewNotifier
{
    Task NotifyConversationUpdate(ConversationUpdate update, CancellationToken cancellationToken = default);
    Task NotifyRequestCreated(RequestCreatedNotification notification, CancellationToken cancellationToken = default);
}
