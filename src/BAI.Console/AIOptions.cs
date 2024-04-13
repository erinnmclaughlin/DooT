namespace BAI.ConsoleApp;

internal sealed class AIOptions
{
    public string ApiKey { get; init; } = "";
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/chat/completions";
    public string ModelId { get; init; } = "gpt-3.5-turbo";
}
