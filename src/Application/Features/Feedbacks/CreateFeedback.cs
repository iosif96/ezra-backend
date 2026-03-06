using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Feedbacks.CreateFeedback;

[Authorize]
public record CreateFeedbackCommand(int ConversationId, int FlightId, int Rating, string? Content, float? SentimentScore) : IRequest<int>;

public class CreateFeedbackCommandValidator : AbstractValidator<CreateFeedbackCommand>
{
    public CreateFeedbackCommandValidator()
    {
        RuleFor(x => x.ConversationId).GreaterThan(0);
        RuleFor(x => x.FlightId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.SentimentScore).InclusiveBetween(-1f, 1f).When(x => x.SentimentScore.HasValue);
    }
}

internal sealed class CreateFeedbackCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateFeedbackCommand, int>
{
    public async Task<int> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        var entity = new Feedback
        {
            ConversationId = request.ConversationId,
            FlightId = request.FlightId,
            Rating = request.Rating,
            Content = request.Content,
            SentimentScore = request.SentimentScore,
        };

        context.Feedbacks.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
