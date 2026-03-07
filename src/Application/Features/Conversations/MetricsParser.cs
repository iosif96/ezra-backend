using System.Text.RegularExpressions;

namespace Application.Features.Conversations;

public record MessageMetrics(float? Stress, float? Satisfaction);

public static partial class MetricsParser
{
    private static readonly Regex MetricsRegex = CreateMetricsRegex();

    public static (string CleanText, MessageMetrics Metrics) Parse(string text)
    {
        var match = MetricsRegex.Match(text);
        if (!match.Success)
        {
            return (text, new MessageMetrics(null, null));
        }

        float? stress = float.TryParse(match.Groups["stress"].Value, out var s) ? s : null;
        float? satisfaction = float.TryParse(match.Groups["satisfaction"].Value, out var sat) ? sat : null;

        var cleanText = text[..match.Index].TrimEnd();

        return (cleanText, new MessageMetrics(stress, satisfaction));
    }

    [GeneratedRegex(@"\[metrics:stress=(?<stress>[\d.]+),satisfaction=(?<satisfaction>[\d.]+)\]\s*$")]
    private static partial Regex CreateMetricsRegex();
}
