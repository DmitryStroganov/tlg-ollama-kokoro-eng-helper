using System.ComponentModel.DataAnnotations;


/// <summary>
/// Application configuration settings bound from appsettings.json and environment variables.
/// </summary>
public class AppSettings
{
    public const string SectionName = "App";

    /// <summary>
    /// Ollama API endpoint URL.
    /// </summary>
    public string OllamaApi { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Telegram bot token.
    /// </summary>
    [Required]
    public string TlgBotToken { get; set; } = string.Empty;

    /// <summary>
    /// Default voice to use for voice sample generation.
    /// </summary>
    public string DefaultVoice { get; set; } = "en_US-lessac-medium";

    public string KokoroSpeechApi { get; set; } = "http://localhost:8880";
}
