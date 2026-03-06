using System.Text.RegularExpressions;

namespace Application.Common.Models.Chat;

public static partial class MessageSplitter
{
    public static List<string> SplitIntoMessages(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        // First split on double newlines (paragraphs)
        var paragraphs = text.Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries);

        var messages = new List<string>();

        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            // If paragraph is short enough, keep it as one message
            if (trimmed.Length <= 200)
            {
                messages.Add(trimmed);
                continue;
            }

            // Split longer paragraphs into sentences
            var sentences = SentencePattern().Split(trimmed);
            var current = "";

            foreach (var sentence in sentences)
            {
                var s = sentence.Trim();
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                // Group short consecutive sentences together
                if (current.Length > 0 && current.Length + s.Length + 1 <= 200)
                {
                    current += " " + s;
                }
                else
                {
                    if (current.Length > 0)
                        messages.Add(current);
                    current = s;
                }
            }

            if (current.Length > 0)
                messages.Add(current);
        }

        return messages;
    }

    [GeneratedRegex(@"(?<=[.!?])\s+", RegexOptions.Compiled)]
    private static partial Regex SentencePattern();
}
