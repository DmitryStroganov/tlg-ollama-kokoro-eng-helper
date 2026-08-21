using Microsoft.Extensions.AI;

public static class PhonologicalService
{
    public static readonly ChatMessage SystemPrompt = new(ChatRole.System, """
                You are a British English phonologist. Analyze the user-provided word/phrase and provide a structured phonological breakdown.

                Provide the following information in JSON format:
                1. IPA transcription (British English pronunciation)
                2. Syllable breakdown with stress markers
                3. Phoneme-by-phoneme breakdown
                4. Primary sentence stress (which syllable/word receives primary stress)
                5. Secondary stresses (which syllables/words receive secondary stress)
                6. Speed/Rhythm (pace and rhythmic pattern)
                7. Tone (overall tonal quality - e.g., formal, conversational, emphatic)
                8. Pacing (how the speech flows - e.g., steady, varied, deliberate)

                Always reply in English.

                Return ONLY valid JSON, no markdown formatting, no code blocks. The JSON should have these exact keys:
                - "ipa": string
                - "syllableBreakdown": string
                - "phonemeBreakdown": string
                - "primaryStress": string
                - "secondaryStresses": string
                - "speedRhythm": string
                - "tone": string
                - "pacing": string
                """);

    /// <summary>
    /// Parses the Ollama response into a PhonologicalBreakdown object.
    /// </summary>
    public static PhonologicalBreakdown ParseResponse(string response)
    {
        var breakdown = new PhonologicalBreakdown
        {
            PrimarySentenceStress = "Default",
            SecondaryStresses = "Default",
            SpeedRhythm = "Default",
            Tone = "Default",
            Pacing = "Default",
        };

        // Try to parse as JSON first
        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("ipa", out var ipaProp))
            {
                breakdown.IpATranscription = ipaProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("syllableBreakdown", out var sylProp))
            {
                breakdown.SyllableBreakdown = sylProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("phonemeBreakdown", out var phonoProp))
            {
                breakdown.PhonemeBreakdown = phonoProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("primaryStress", out var primProp))
            {
                breakdown.PrimarySentenceStress = primProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("secondaryStresses", out var secProp))
            {
                breakdown.SecondaryStresses = secProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("speedRhythm", out var speedProp))
            {
                breakdown.SpeedRhythm = speedProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("tone", out var toneProp))
            {
                breakdown.Tone = toneProp.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("pacing", out var paceProp))
            {
                breakdown.Pacing = paceProp.GetString() ?? string.Empty;
            }

            return breakdown;
        }
        catch
        {
            // Fallback: use raw response if JSON parsing fails
            breakdown.IpATranscription = response.Trim();
            return breakdown;
        }
    }
}
