using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Feedbacks.UpdateFeedback;

[Authorize]
public record UpdateFeedbackCommand(int Id, int Rating, string? Content, float? SentimentScore) : IRequest;

public class UpdateFeedbackCommandValidator : AbstractValidator<UpdateFeedbackCommand>
{
    public UpdateFeedbackCommandValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.SentimentScore).InclusiveBetween(-1f, 1f).When(x => x.SentimentScore.HasValue);
    }
}

internal sealed class UpdateFeedbackCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateFeedbackCommand>
{
    public async Task Handle(UpdateFeedbackCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Feedbacks
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Feedback), request.Id);

        entity.Rating = request.Rating;
        entity.Content = request.Content;
        entity.SentimentScore = request.SentimentScore;

        await context.SaveChangesAsync(cancellationToken);
    }
}
