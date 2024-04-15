namespace DooT;

internal sealed class Application(Chatbot doot)
{
    private readonly Chatbot _doot = doot;

    public async Task Run()
    {
        WriteInitialMessageHistory();

        while (TryAddUserResponse())
        {
            await GenerateAssistantResponse();
        }
    }

    private async Task GenerateAssistantResponse()
    {
        var dootResponse = await _doot.GenerateResponse();

        WriteNameLabel("DooT");
        WriteMessage(dootResponse.Content);
    }

    private bool TryAddUserResponse()
    {
        WriteNameLabel("User");
        var userResponse = Console.ReadLine()?.Trim();
        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(userResponse) || userResponse.Equals("quit", StringComparison.OrdinalIgnoreCase))
            return false;

        _doot.AddUserMessage(userResponse);
        return true;
    }

    private void WriteInitialMessageHistory()
    {
        foreach (var (author, message) in _doot.GetChatHistory())
        {
            WriteNameLabel(author);
            WriteMessage(message);
        }
    }

    private static void WriteNameLabel(string authorName)
    {
        Console.ForegroundColor = authorName == "DooT" ? ConsoleColor.Magenta : ConsoleColor.Cyan;
        Console.WriteLine(authorName);
        Console.ResetColor();
    }

    private static void WriteMessage(string? content)
    {
        Console.WriteLine(content);
        Console.WriteLine();
    }
}
