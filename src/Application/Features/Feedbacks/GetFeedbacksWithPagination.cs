using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Feedbacks.GetFeedbacksWithPagination;

public record FeedbackBriefResponse(int Id, int ConversationId, int FlightId, int Rating, float? SentimentScore);

[Authorize]
public record GetFeedbacksWithPaginationQuery(int? ConversationId = null, int? FlightId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<FeedbackBriefResponse>>;

internal sealed class GetFeedbacksWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetFeedbacksWithPaginationQuery, PaginatedList<FeedbackBriefResponse>>
{
    public Task<PaginatedList<FeedbackBriefResponse>> Handle(GetFeedbacksWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Feedbacks
            .Where(x => request.ConversationId == null || x.ConversationId == request.ConversationId)
            .Where(x => request.FlightId == null || x.FlightId == request.FlightId)
            .OrderByDescending(x => x.Id)
            .Select(x => new FeedbackBriefResponse(x.Id, x.ConversationId, x.FlightId, x.Rating, x.SentimentScore))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
