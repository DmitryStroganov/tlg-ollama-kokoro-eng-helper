using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TelegramVoiceOperator
{
    private readonly ILogger<TelegramVoiceOperator> _logger;

    private readonly TelegramBotClient _botClient;

    public TelegramVoiceOperator(ILogger<TelegramVoiceOperator> logger, AppSettings settings, TelegramBotClient botClient)
    {
        _logger = logger;
        Guard.IsNotNullOrWhiteSpace(settings.TlgBotToken);
        _botClient = botClient;
    }

    /// <summary>
    /// Sends an OGG audio file as a Voice message.
    /// </summary>
    public async Task SendVoice(long userChatId, Stream audioData, string? caption = null, int? messageId = null)
    {
        if (messageId != null)
        {
            var replyParameters = new ReplyParameters()
            {
                MessageId = messageId,
            };

            var sendVoiceResult = await _botClient.SendVoice(
                chatId: userChatId,
                voice: new InputFileStream(audioData),
                caption: caption,
                replyParameters: replyParameters,
                parseMode: ParseMode.None);

            _logger.LogInformation($"Voice message sent! Message ID: {sendVoiceResult?.Id}");
        }
        else
        {
            var sendVoiceResult = await _botClient.SendVoice(
                chatId: userChatId,
                voice: new InputFileStream(audioData),
                caption: caption,
                parseMode: ParseMode.None);

            _logger.LogInformation($"Voice message sent! Message ID: {sendVoiceResult?.Id}");
        }
    }

    /// <summary>
    /// Shortcut: sends Audio message.
    /// </summary>
    public async Task SendAudio(long userChatId, string filePath, string caption = null)
    {
        var audioFileInfo = new FileInfo(filePath);

        if (!audioFileInfo.Exists)
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        }

        var sendResult = await _botClient.SendAudio(
                chatId: userChatId,
                audio: audioFileInfo.OpenRead(),
                caption: caption
                );

        _logger.LogInformation($"Audio message sent! Message ID: {sendResult?.Id}");
    }

    public async Task SendAudio(long userChatId, Stream audioData, string caption = null)
    {
        var sendResult = await _botClient.SendAudio(
                chatId: userChatId,
                audio: audioData,
                caption: caption
                );

        _logger.LogInformation($"Audio message sent! Message ID: {sendResult?.Id}");
    }
}
