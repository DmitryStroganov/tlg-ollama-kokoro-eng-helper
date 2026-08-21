using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);
        builder.Services.AddConfiguration<AppSettings>(builder.Configuration);
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<TelegramBotClient>(sp =>
        {
            return new TelegramBotClient(sp.GetRequiredService<AppSettings>().TlgBotToken);
        });

        builder.Services.AddSingleton<ITlgCommandHandlerService, TlgCommandHandlerService>();
        builder.Services.AddSingleton<TelegramBotService>();
        builder.Services.AddSingleton<TelegramVoiceOperator>();
        builder.Services.AddSingleton<KokoroSpeechClient>();
        try
        {
            var app = builder.Build();

            var telegramBot = app.Services.GetRequiredService<TelegramBotService>();

            Console.WriteLine("=== TLG Bot - Phonological Analyzer ===");
            Console.WriteLine();

            Console.CancelKeyPress += async (_, e) =>
            {
                e.Cancel = true;
                await telegramBot.StopAsync();
                telegramBot.CancellationSource.Cancel();
            };

            await telegramBot.StartAsync();

            await Task.Delay(-1, telegramBot.CancellationSource.Token);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Bot stopped.");
        }
    }
}