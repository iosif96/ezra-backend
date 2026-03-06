namespace Application.Common.Exceptions;

/// <summary>
/// Base exception for Gemini service related errors
/// </summary>
public class GeminiException : Exception
{
    public string? ErrorCode { get; }
    public int? StatusCode { get; }

    public GeminiException(string message, string? errorCode = null, int? statusCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public GeminiException(string message, Exception innerException, string? errorCode = null, int? statusCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Exception thrown when the Gemini API blocks a request due to safety concerns
/// </summary>
public class GeminiSafetyException : GeminiException
{
    public string BlockReason { get; }

    public GeminiSafetyException(string blockReason)
        : base($"Request blocked due to safety concerns: {blockReason}", "SAFETY_BLOCK")
    {
        BlockReason = blockReason;
    }
}

/// <summary>
/// Exception thrown when Gemini API rate limits are exceeded
/// </summary>
public class GeminiRateLimitException : GeminiException
{
    public TimeSpan? RetryAfter { get; }

    public GeminiRateLimitException(TimeSpan? retryAfter = null)
        : base("Gemini API rate limit exceeded", "RATE_LIMIT", 429)
    {
        RetryAfter = retryAfter;
    }
}