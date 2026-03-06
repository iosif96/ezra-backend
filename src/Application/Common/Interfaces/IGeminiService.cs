using Application.Common.Models.Gemini;

namespace Application.Common.Interfaces;

/// <summary>
/// Service for interacting with Google's Gemini AI API
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Sends a chat request to the Gemini API
    /// </summary>
    /// <param name="request">The chat request containing messages and options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the Gemini API</returns>
    Task<GeminiChatResponse> SendChatAsync(
        GeminiChatRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a simple text message to the Gemini API
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The text response from the Gemini API</returns>
    Task<string> SendMessageAsync(
        string message,
        CancellationToken cancellationToken = default);
}