/// <summary>
/// Represents the phonological breakdown result for a given word/phrase.
/// </summary>
public class PhonologicalBreakdown
{
    /// <summary>
    /// The IPA transcription in British English.
    /// </summary>
    public string IpATranscription { get; set; } = string.Empty;

    /// <summary>
    /// Syllable breakdown with stress markers.
    /// </summary>
    public string SyllableBreakdown { get; set; } = string.Empty;

    /// <summary>
    /// Phoneme-by-phoneme breakdown.
    /// </summary>
    public string PhonemeBreakdown { get; set; } = string.Empty;

    /// <summary>
    /// Primary sentence stress information.
    /// </summary>
    public string PrimarySentenceStress { get; set; } = string.Empty;

    /// <summary>
    /// Secondary stress information.
    /// </summary>
    public string SecondaryStresses { get; set; } = string.Empty;

    /// <summary>
    /// Speed and rhythm information.
    /// </summary>
    public string SpeedRhythm { get; set; } = string.Empty;

    /// <summary>
    /// Tone direction.
    /// </summary>
    public string Tone { get; set; } = string.Empty;

    /// <summary>
    /// Pacing direction.
    /// </summary>
    public string Pacing { get; set; } = string.Empty;

    /// <summary>
    /// Generates the formatted output string.
    /// </summary>
    public string FormatOutput()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Phonological Breakdown");
        sb.AppendLine();
        sb.AppendLine(IpATranscription);
        sb.AppendLine();
        sb.AppendLine(SyllableBreakdown);
        sb.AppendLine();
        sb.AppendLine(PhonemeBreakdown);
        sb.AppendLine();

        sb.AppendLine("Prosody & Pitch Contour");
        sb.AppendLine($"- Primary Sentence Stress: {PrimarySentenceStress}");
        sb.AppendLine($"- Secondary Stresses: {SecondaryStresses}");
        sb.AppendLine($"- Speed/Rhythm: {SpeedRhythm}");
        sb.AppendLine();

        sb.AppendLine("Voice Direction");
        sb.AppendLine($"- Tone: {Tone}");
        sb.AppendLine($"- Pacing: {Pacing}");
        sb.AppendLine();
        sb.AppendLine();

        return sb.ToString();
    }
}
