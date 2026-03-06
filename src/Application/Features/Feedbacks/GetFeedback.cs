using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Feedbacks.GetFeedback;

public record FeedbackResponse(int Id, int ConversationId, int FlightId, int Rating, string? Content, float? SentimentScore);

[Authorize]
public record GetFeedbackQuery(int Id) : IRequest<FeedbackResponse>;

internal sealed class GetFeedbackQueryHandler(ApplicationDbContext context) : IRequestHandler<GetFeedbackQuery, FeedbackResponse>
{
    public async Task<FeedbackResponse> Handle(GetFeedbackQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Feedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Feedback), request.Id);

        return new FeedbackResponse(entity.Id, entity.ConversationId, entity.FlightId, entity.Rating, entity.Content, entity.SentimentScore);
    }
}
