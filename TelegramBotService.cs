using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class TelegramBotService
{
    private readonly TelegramBotClient _botClient;
    private readonly ITlgCommandHandlerService _tlgCommandHandlerService;
    private readonly ILogger<TelegramBotService> _logger;
    public readonly CancellationTokenSource CancellationSource;

    public TelegramBotService(ITlgCommandHandlerService tlgCommandHandlerService, AppSettings settings, ILogger<TelegramBotService> logger)
    {
        _tlgCommandHandlerService = tlgCommandHandlerService;
        _logger = logger;
        CancellationSource = new CancellationTokenSource();
        Guard.IsNotNullOrWhiteSpace(settings.TlgBotToken);
        _botClient = new TelegramBotClient(settings.TlgBotToken, cancellationToken: CancellationSource.Token);
    }

    public TelegramBotService(ITlgCommandHandlerService tlgCommandHandlerService, AppSettings settings, TelegramBotClient botClient, ILogger<TelegramBotService> logger)
    {
        _tlgCommandHandlerService = tlgCommandHandlerService;
        _logger = logger;
        CancellationSource = new CancellationTokenSource();
        Guard.IsNotNullOrWhiteSpace(settings.TlgBotToken);
        _botClient = botClient;
    }

    /// <summary>
    /// Starts the bot polling for messages.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== Tlg Ollama Chat Bot : eng-word-spell ===");

        // Receiver Options for long polling
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery]
        };

        // Start receiving updates
        _botClient.StartReceiving(
            updateHandler: HandleUpdate,
            errorHandler: HandleError,
            receiverOptions: receiverOptions
        );

        //await _botClient.SetMyCommands([("/help", "Command description")], BotCommandScope.AllPrivateChats());

        _logger.LogInformation("Telegram bot is running. Press Ctrl+C to stop.");
    }

    /// <summary>
    /// Stops the bot polling.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Telegram bot stopping...");
        await _botClient.DropPendingUpdates(cancellationToken);
        //await _botClient.Close();
    }

    /// <summary>
    /// Handles incoming updates from Telegram.
    /// </summary>
    private async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message is { Text: { } userMessage } message)
            {
                var chatId = message.Chat.Id;
                var text = message.Text;

                _logger.LogInformation($"[{message.Chat.Id}] User: '{userMessage}'.");

                // Indicate typing state to Telegram user
                await _botClient.SendChatAction(message.Chat.Id, ChatAction.Typing, cancellationToken: cancellationToken);

                // Parse command
                if (text.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var command = parts[0][1..].ToLowerInvariant();
                    var arguments = parts.Length > 1 ? string.Join(" ", parts[1..]) : null;

                    switch (command)
                    {
                        case "analyze":
                            await _tlgCommandHandlerService.HandleAnalyzeCommand(chatId, arguments ?? text.Trim(), message.MessageId, cancellationToken);
                            break;
                        case "explain":
                            await _tlgCommandHandlerService.HandleExplainCommand(chatId, arguments ?? text.Trim(), message.MessageId, cancellationToken);
                            break;
                        case "spell":
                            await _tlgCommandHandlerService.HandleSpellCommand(chatId, arguments ?? text.Trim(), message.MessageId, cancellationToken);
                            break;
                        case "help":
                            await _tlgCommandHandlerService.HandleHelpCommand(chatId, cancellationToken);
                            //await botClient.SetChatMenuButton(chatId,  , cancellationToken);
                            break;
                        default:
                            await SendMessage(_botClient, chatId, $"Unknown command: {command}. Use /help for available commands.", cToken: cancellationToken);
                            break;
                    }
                }
                else
                {
                    // Default action
                    await _tlgCommandHandlerService.HandleSpellCommand(chatId, text.Trim(), message.MessageId, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }


    public static async Task ReplyMessage(TelegramBotClient botClient, int messageId, long chatId, string text, ParseMode parseMode = ParseMode.None, CancellationToken cToken = default)
    {
        var replyParameters = new ReplyParameters()
        {
            MessageId = messageId,
        };

        await botClient.SendMessage(chatId, text, parseMode: parseMode, replyParameters: replyParameters, cancellationToken: cToken);
    }

    /// <summary>
    /// Sends a message to the specified chat.
    /// </summary>
    public static async Task SendMessage(TelegramBotClient botClient, long chatId, string text, ParseMode parseMode = ParseMode.None, ReplyMarkup? replyMarkup = null, CancellationToken cToken = default)
    {
        // Split long messages if needed
        if (text.Length <= 4096)
        {
            await botClient.SendMessage(chatId, text, parseMode: parseMode, replyMarkup: replyMarkup, cancellationToken: cToken);
        }
        else
        {
            // Split into chunks
            var chunks = SplitText(text, 4096);
            foreach (var chunk in chunks)
            {
                await botClient.SendMessage(chatId, chunk, parseMode: parseMode, cancellationToken: cToken);
            }
        }
    }

    /// <summary>
    /// Splits text into chunks of the specified maximum length.
    /// </summary>
    private static List<string> SplitText(string text, int maxLength)
    {
        var chunks = new List<string>();
        var remaining = text;

        while (remaining.Length > maxLength)
        {
            var splitPos = remaining.AsSpan().LastIndexOfAny(new[] { '\n', '\r', '\t', ' ' });
            if (splitPos < 0)
            {
                splitPos = maxLength;
            }

            chunks.Add(remaining.Substring(0, splitPos));
            remaining = remaining.Substring(splitPos).TrimStart();
        }

        if (remaining.Length > 0)
        {
            chunks.Add(remaining);
        }

        return chunks;
    }

    /// <summary>
    /// Handles errors from the bot.
    /// </summary>
    private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram bot error");
        return Task.CompletedTask;
    }
}
