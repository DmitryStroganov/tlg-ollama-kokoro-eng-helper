using Microsoft.Extensions.AI;

public static partial class WordExplainerService
{
    public static readonly ChatMessage SystemPrompt = new(ChatRole.System, """
                Task: Your goal is to provide explanation of an English word or phrase, for non-native speakers, by describing the meaning using most related synonyms, and providing an usage example.
                
                Requirements:
                Do not invent citations, evidence, or data.
                Ask for clarification if context is ambiguous.
                If request is not in English - use nearest English equivalent.
                Always reply in English.

                Format the output into three sections: Term, Synonyms, Meaning, Example.
                Return ONLY valid JSON, no markdown formatting, no code blocks. The JSON should have these exact keys:
                - "Term": string
                - "Synonyms": string
                - "Meaning": string
                - "Example": string
                
                Treat each user request as an isolated, single-use task. 
                Once you deliver the result below, do not carry forward, reference, or incorporate any facts, variables, or context from this turn into subsequent responses unless explicitly re-provided.
                """);

    public static WordExplainerBreakdown ParseResponse(string response)
    {
        var breakdown = new WordExplainerBreakdown();

        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            {
                if (root.TryGetProperty("Term", out var dataProp))
                {
                    breakdown.Term = dataProp.GetString() ?? string.Empty;
                }
            }

            {
                if (root.TryGetProperty("Synonyms", out var dataProp))
                {
                    breakdown.Synonyms = dataProp.GetString() ?? string.Empty;
                }
            }

            {
                if (root.TryGetProperty("Meaning", out var dataProp))
                {
                    breakdown.Meaning = dataProp.GetString() ?? string.Empty;
                }
            }

            {
                if (root.TryGetProperty("Example", out var dataProp))
                {
                    breakdown.Example = dataProp.GetString() ?? string.Empty;
                }
            }

            return breakdown;
        }
        catch
        {
            // Fallback: use raw response if JSON parsing fails
            breakdown.Meaning = response.Trim();
            return breakdown;
        }
    }
}