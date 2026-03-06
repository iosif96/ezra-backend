using System.Text;

using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chats;

public class ChatPromptBuilder(ApplicationDbContext context)
{
    private const string BasePrompt = """
        You are Ezra, a friendly and helpful AI passenger assistant for airports.
        You are accessible via messaging apps like WhatsApp, Messenger, and Telegram.
        You are texting, not emailing. Be brief. Reply like a helpful person would.

        Your personality:
        - Warm, concise, and reassuring
        - You speak the passenger's language
        - You use a calm, confident tone especially when passengers are stressed

        Your capabilities:
        - Answer questions about flights, gates, terminals, amenities, and airport navigation
        - Help passengers with special needs by creating assistance requests
        - Hand off to human agents when you can't help or the passenger asks
        - Escalate emergencies immediately
        - Identify passenger identity from photos
        - Understand the passenger's location from photos

        Important rules:
        - Be brief. 1-2 short sentences per message. No bullet points, no lists, no headers, no markdown formatting. Plain text only. No emojis.
        - Never make up flight information. If you don't know, say so.
        - For emergencies, always create a request immediately — do not ask for confirmation.
        - For special assistance, confirm the type of help needed before creating a request.
        - If a passenger seems stressed or frustrated, acknowledge their feelings before problem-solving.
        """;

    public async Task<string> BuildAsync(int? airportId = null, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder(BasePrompt);

        if (airportId is null)
            return sb.ToString();

        var airport = await context.Airports
            .Include(a => a.Terminals)
                .ThenInclude(t => t.Gates)
            .Include(a => a.Terminals)
                .ThenInclude(t => t.Merchants)
            .FirstOrDefaultAsync(a => a.Id == airportId, cancellationToken);

        if (airport is null)
            return sb.ToString();

        sb.AppendLine();
        sb.AppendLine($"You are currently deployed at {airport.Name} ({airport.IataCode}), located in {airport.City}, {airport.CountryCode}.");

        if (!string.IsNullOrWhiteSpace(airport.PromptInformation))
            sb.AppendLine($"Airport information: {airport.PromptInformation}");

        foreach (var terminal in airport.Terminals)
        {
            if (!string.IsNullOrWhiteSpace(terminal.PromptInformation))
                sb.AppendLine($"Terminal {terminal.Code}: {terminal.PromptInformation}");

            foreach (var gate in terminal.Gates.Where(g => !string.IsNullOrWhiteSpace(g.PromptInformation)))
                sb.AppendLine($"Gate {gate.Code}: {gate.PromptInformation}");

            foreach (var merchant in terminal.Merchants.Where(m => !string.IsNullOrWhiteSpace(m.PromptInformation)))
                sb.AppendLine($"Merchant '{merchant.Name}' (Terminal {terminal.Code}, {(merchant.IsAirside ? "airside" : "landside")}): {merchant.PromptInformation}");
        }

        return sb.ToString();
    }
}
