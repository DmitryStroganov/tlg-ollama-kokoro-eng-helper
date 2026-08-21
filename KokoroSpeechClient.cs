using System.Net.Http.Json;
using CommunityToolkit.Diagnostics;

public class KokoroSpeechClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public KokoroSpeechClient(IHttpClientFactory httpClientFactory, AppSettings settings)
    {
        _httpClient = httpClientFactory.CreateClient();
        Guard.IsNotNullOrWhiteSpace(settings.KokoroSpeechApi);
        _baseUrl = settings.KokoroSpeechApi;
    }

    public class SpeechRequest
    {
        public string Model { get; set; } = "kokoro";
        public string Input { get; set; }
        public string Voice { get; set; } = "bf_lily";
        public string ResponseFormat { get; set; } = "opus";
    }

    public async Task<byte[]> SynthesizeVoice(
        string text,
        string voice = "bf_lily",
        string model = "kokoro",
        string format = "opus")
    {
        var request = new SpeechRequest
        {
            Model = model,
            Input = text,
            Voice = voice,
            ResponseFormat = format
        };

        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/v1/audio/speech", request);
        response.EnsureSuccessStatusCode();

        //var filename = $"kokoro_{Guid.NewGuid():N}.{format}";
        return await response.Content.ReadAsByteArrayAsync();
    }
}
