
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class TlgCommandHandlerService : ITlgCommandHandlerService
{
    private readonly ILogger<TlgCommandHandlerService> _logger;
    private readonly AppSettings _settings;
    private readonly KokoroSpeechClient _kokoroSpeechClient;
    private readonly TelegramVoiceOperator _telegramVoiceOperator;
    private readonly TelegramBotClient _botClient;
    private readonly IChatClient _chatClient;

    public TlgCommandHandlerService(ILogger<TlgCommandHandlerService> logger, AppSettings settings, KokoroSpeechClient kokoroSpeechClient, TelegramVoiceOperator telegramVoiceOperator, TelegramBotClient botClient)
    {
        _logger = logger;
        _settings = settings;
        _kokoroSpeechClient = kokoroSpeechClient;
        _telegramVoiceOperator = telegramVoiceOperator;
        _botClient = botClient;

        var ollamaUri = settings.OllamaApi ?? "http://localhost:11434";
        _chatClient = new OllamaApiClient(
            new Uri(ollamaUri),
            "mo-shakib/clearwriter:latest"
        );
    }

    public async Task HandleHelpCommand(long chatId, CancellationToken cancellationToken)
    {
        const string message = """
        <b><u>Bot menu</u></b>:

        /analyze [text] - Analyze phonological properties of the word.
        /explain [text] - Explanation of an English word or phrase.
        /spell [text] - TTS.
        /help - Show this help message.

        Default: TTS.
        """;

        await TelegramBotService.SendMessage(_botClient, chatId, message, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove(), cancellationToken);
    }

    public async Task HandleAnalyzeCommand(long chatId, string text, int? messageId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogError("No request provided.");
            return;
        }

        try
        {
            // total lenght limit
            if (text.Length > 50)
            {
                await TelegramBotService.SendMessage(_botClient, chatId, "The text provided is too long.", cToken: cancellationToken);
                _logger.LogWarning("The text provided is too long.");
                return;
            }

            bool hasMultipleWords = text.AsSpan().IndexOfAny(' ', '\t', '\r') >= 0;

            if (hasMultipleWords)
            {
                await TelegramBotService.SendMessage(_botClient, chatId, "Only single word request is expected.", cToken: cancellationToken);
                _logger.LogWarning("Multiple words provided.");
                return;
            }
            else
            {
                List<ChatMessage> promptData = [PhonologicalService.SystemPrompt, new ChatMessage(ChatRole.User, text)];

                var responseBuilder = new StringBuilder();

                // usage data
                UsageDetails? usage = null;

                // Stream tokens in real time using IChatClient
                await foreach (var response in _chatClient.GetStreamingResponseAsync(promptData))
                {
                    responseBuilder.Append(response.Text);

                    // Capture usage
                    foreach (var content in response.Contents)
                    {
                        if (content is UsageContent usageContent)
                        {
                            usage = usageContent.Details;
                        }
                    }
                }

                var responseText = responseBuilder.ToString();

                var breakdown = PhonologicalService.ParseResponse(responseText);

                if (messageId != null)
                {
                    await TelegramBotService.ReplyMessage(_botClient, messageId: messageId!.Value, chatId, breakdown.FormatOutput(), cToken: cancellationToken);
                }
                else
                {
                    await TelegramBotService.SendMessage(_botClient, chatId, breakdown.FormatOutput(), cToken: cancellationToken);
                }

                _logger.LogInformation($"Chat: {chatId} Tokens: {usage?.InputTokenCount}/{usage?.OutputTokenCount}.");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error analyzing text: {ex.Message}");
        }
    }

    public async Task HandleExplainCommand(long chatId, string text, int? messageId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogError("No request provided.");
            return;
        }

        try
        {
            // total lenght limit
            if (text.Length > 100)
            {
                await TelegramBotService.SendMessage(_botClient, chatId, "The text provided is too long.", cToken: cancellationToken);
                _logger.LogWarning("The text provided is too long.");
                return;
            }

            List<ChatMessage> promptData = [WordExplainerService.SystemPrompt, new ChatMessage(ChatRole.User, text)];

            var responseBuilder = new StringBuilder();

            // usage data
            UsageDetails? usage = null;

            // Stream tokens in real time using IChatClient
            await foreach (var response in _chatClient.GetStreamingResponseAsync(promptData))
            {
                responseBuilder.Append(response.Text);

                // Capture usage
                foreach (var content in response.Contents)
                {
                    if (content is UsageContent usageContent)
                    {
                        usage = usageContent.Details;
                    }
                }
            }

            var responseText = responseBuilder.ToString();

            var breakdown = WordExplainerService.ParseResponse(responseText);

            if (messageId != null)
            {
                await TelegramBotService.ReplyMessage(_botClient, messageId: messageId!.Value, chatId, breakdown.FormatOutput(), cToken: cancellationToken);
            }
            else
            {
                await TelegramBotService.SendMessage(_botClient, chatId, breakdown.FormatOutput(), cToken: cancellationToken);
            }

            _logger.LogInformation($"Chat: {chatId} Tokens: {usage?.InputTokenCount}/{usage?.OutputTokenCount}.");
            return;

        }
        catch (Exception ex)
        {
            _logger.LogError($"Error analyzing text: {ex.Message}");
        }
    }

    public async Task HandleSpellCommand(long chatId, string text, int? messageId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogError("No request provided.");
            return;
        }

        // total lenght limit
        if (text.Length > 300)
        {
            await TelegramBotService.SendMessage(_botClient, chatId, "The text provided is too long.", cToken: cancellationToken);
            _logger.LogWarning("The text provided is too long.");
            return;
        }

        var voiceData = await _kokoroSpeechClient.SynthesizeVoice(text);
        Stream stream = new MemoryStream(voiceData);
        await _telegramVoiceOperator.SendVoice(chatId, stream, caption: null, messageId);
        return;
    }
}