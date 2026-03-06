using Application.Common.Models.Chat;

namespace Application.Common.Interfaces;

public interface IChatCompletionService
{
    Task<ChatCompletionResult> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}
