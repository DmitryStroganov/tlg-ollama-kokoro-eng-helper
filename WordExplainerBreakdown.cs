public static partial class WordExplainerService
{
    public class WordExplainerBreakdown
    {
        public string Term { get; set; } = string.Empty;
        public string Synonyms { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;

        public string FormatOutput()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"Term: {Term}");
            sb.AppendLine();

            sb.AppendLine($"Synonyms: {Synonyms}");
            sb.AppendLine();

            sb.AppendLine($"Meaning: {Meaning}");
            sb.AppendLine();

            sb.AppendLine($"Example: {Example}");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}