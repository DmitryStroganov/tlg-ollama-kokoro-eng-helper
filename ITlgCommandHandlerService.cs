public interface ITlgCommandHandlerService
{
    Task HandleHelpCommand(long chatId, CancellationToken cancellationToken);
    Task HandleAnalyzeCommand(long chatId, string text, int? messageId, CancellationToken cancellationToken);
    Task HandleExplainCommand(long chatId, string text, int? messageId, CancellationToken cancellationToken);
    Task HandleSpellCommand(long chatId, string text, int? messageId, CancellationToken cancellationToken);
}
