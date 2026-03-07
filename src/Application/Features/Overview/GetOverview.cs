using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Overview.GetOverview;

public record ChannelTypeCount(ChannelType ChannelType, int Count);

public record TouchpointScansOverview(
    int CountLastHour,
    int CountToday,
    List<ChannelTypeCount> CountTodayByChannelType);

public record ConversationsOverview(
    int MessagesLastHour,
    int ConversationsLastHour,
    int ConversationsToday,
    double? AverageLlmResponseTimeSeconds);

public record SatisfactionDataPoint(DateOnly Date, double AverageRating, int Count);

public record StressDataPoint(DateOnly Date, double AverageStress, int Count);

public record ChartsOverview(
    List<SatisfactionDataPoint> SatisfactionIndex,
    double? SatisfactionAverage,
    List<StressDataPoint> StressIndex,
    double? StressAverage);

public record RequestOverviewItem(
    int Id,
    RequestType Type,
    string? Content,
    RequestStatus Status,
    int? StaffId,
    string? StaffName,
    ChannelType ChannelType,
    DateTime Created);

public record OverviewResponse(
    TouchpointScansOverview TouchpointScans,
    ConversationsOverview Conversations,
    ChartsOverview Charts,
    List<RequestOverviewItem> Requests);

[Authorize]
public record GetOverviewQuery : IRequest<OverviewResponse>;

internal sealed class GetOverviewQueryHandler(ApplicationDbContext context) : IRequestHandler<GetOverviewQuery, OverviewResponse>
{
    public async Task<OverviewResponse> Handle(GetOverviewQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var oneHourAgo = now.AddHours(-1);
        var todayStart = now.Date;
        var thirtyDaysAgo = todayStart.AddDays(-30);

        var touchpointScans = await GetTouchpointScansOverview(oneHourAgo, todayStart, cancellationToken);
        var conversations = await GetConversationsOverview(oneHourAgo, todayStart, cancellationToken);
        var charts = await GetChartsOverview(thirtyDaysAgo, cancellationToken);
        var requests = await GetLatestRequests(cancellationToken);

        return new OverviewResponse(touchpointScans, conversations, charts, requests);
    }

    private async Task<TouchpointScansOverview> GetTouchpointScansOverview(
        DateTime oneHourAgo, DateTime todayStart, CancellationToken cancellationToken)
    {
        var countLastHour = await context.TouchpointScans
            .CountAsync(s => s.Created >= oneHourAgo, cancellationToken);

        var countToday = await context.TouchpointScans
            .CountAsync(s => s.Created >= todayStart, cancellationToken);

        var byChannelType = await context.TouchpointScans
            .Where(s => s.Created >= todayStart)
            .GroupBy(s => s.ChannelType)
            .Select(g => new ChannelTypeCount(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return new TouchpointScansOverview(countLastHour, countToday, byChannelType);
    }

    private async Task<ConversationsOverview> GetConversationsOverview(
        DateTime oneHourAgo, DateTime todayStart, CancellationToken cancellationToken)
    {
        var messagesLastHour = await context.Messages
            .CountAsync(m => m.Created >= oneHourAgo, cancellationToken);

        var conversationsLastHour = await context.Conversations
            .CountAsync(c => c.LastMessageOn >= oneHourAgo, cancellationToken);

        var conversationsToday = await context.Conversations
            .CountAsync(c => c.LastMessageOn >= todayStart, cancellationToken);

        // Average LLM response time: time between a User message and the next Assistant message
        var todayMessages = await context.Messages
            .Where(m => m.Created >= todayStart && (m.Type == MessageType.User || m.Type == MessageType.Assistant))
            .OrderBy(m => m.ConversationId)
            .ThenBy(m => m.Created)
            .Select(m => new { m.ConversationId, m.Type, m.Created })
            .ToListAsync(cancellationToken);

        double? avgResponseTime = null;
        var responseTimes = new List<double>();

        for (var i = 1; i < todayMessages.Count; i++)
        {
            var prev = todayMessages[i - 1];
            var curr = todayMessages[i];

            if (prev.ConversationId == curr.ConversationId
                && prev.Type == MessageType.User
                && curr.Type == MessageType.Assistant)
            {
                responseTimes.Add((curr.Created - prev.Created).TotalSeconds);
            }
        }

        if (responseTimes.Count > 0)
        {
            avgResponseTime = Math.Round(responseTimes.Average(), 1);
        }

        return new ConversationsOverview(messagesLastHour, conversationsLastHour, conversationsToday, avgResponseTime);
    }

    private async Task<ChartsOverview> GetChartsOverview(DateTime thirtyDaysAgo, CancellationToken cancellationToken)
    {
        var rawSatisfaction = await context.Feedbacks
            .Where(f => f.Created >= thirtyDaysAgo)
            .GroupBy(f => f.Created.Date)
            .Select(g => new { Date = g.Key, Avg = g.Average(f => f.Rating), Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var satisfactionData = rawSatisfaction
            .Select(x => new SatisfactionDataPoint(DateOnly.FromDateTime(x.Date), Math.Round(x.Avg, 1), x.Count))
            .ToList();

        var satisfactionAverage = satisfactionData.Count > 0
            ? Math.Round(satisfactionData.Average(x => x.AverageRating), 1)
            : (double?)null;

        var rawStress = await context.Messages
            .Where(m => m.Created >= thirtyDaysAgo && m.StressIndex != null)
            .GroupBy(m => m.Created.Date)
            .Select(g => new { Date = g.Key, Avg = g.Average(m => (double)m.StressIndex!), Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var stressData = rawStress
            .Select(x => new StressDataPoint(DateOnly.FromDateTime(x.Date), Math.Round(x.Avg, 2), x.Count))
            .ToList();

        var stressAverage = stressData.Count > 0
            ? Math.Round(stressData.Average(x => x.AverageStress), 2)
            : (double?)null;

        return new ChartsOverview(satisfactionData, satisfactionAverage, stressData, stressAverage);
    }

    private async Task<List<RequestOverviewItem>> GetLatestRequests(CancellationToken cancellationToken)
    {
        return await context.Requests
            .OrderByDescending(r => r.Created)
            .Take(20)
            .Select(r => new RequestOverviewItem(
                r.Id,
                r.Type,
                r.Content,
                r.Status,
                r.StaffId,
                r.Staff != null ? r.Staff.Name : null,
                r.Conversation.ChannelType,
                r.Created))
            .ToListAsync(cancellationToken);
    }
}
