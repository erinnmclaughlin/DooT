using BAI.ConsoleApp;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;

namespace DooT;

internal sealed class Chatbot
{
    private readonly OpenAIPromptExecutionSettings _aiSettings;
    private readonly Kernel _kernel;
    private readonly ILogger<Application> _logger;
    private readonly ChatHistory _messages = [];
    
    public Chatbot(Kernel kernel, ILogger<Application> logger)
    {
        _aiSettings = new()
        {
            MaxTokens = 250,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        _kernel= kernel;
        _kernel.Plugins.AddFromObject(new CustomInteractions());

        _logger = logger;

        InitializeChatHistory();
    }

    public void AddUserMessage(string message)
    {
        _messages.AddUserMessage(message);
    }

    public async Task<ChatMessageContent> GenerateResponse()
    {
        try
        {
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            var response = await chatService.GetChatMessageContentAsync(_messages, _aiSettings, _kernel);

            if (!string.IsNullOrWhiteSpace(response.Content))
                _messages.AddAssistantMessage(response.Content);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate response.");

            var response = new ChatMessageContent(AuthorRole.Assistant, "My apologies, but I encountered an unexpected error.");
            _messages.Add(response);
            return response;
        }
    }

    public IEnumerable<(string Author, string? Message)> GetChatHistory()
    {
        foreach (var message in _messages)
        {
            if (message.Role == AuthorRole.Assistant)
                yield return ("DooT", message.Content);

            else if (message.Role == AuthorRole.User)
                yield return ("User", message.Content);
        }
    }

    private void InitializeChatHistory()
    {
        if (_messages.Count != 0)
            throw new InvalidOperationException("Chat history has already been initialized.");

        _messages.AddSystemMessage("""
            You are an AI personal assistant for a productivity application.
            Your job is to manage the user's todo list. You may also answer questions the user has about the service.
            You try to be concise and only provide longer responses if necessary.
            """);

        _messages.AddAssistantMessage("Hi! I'm DooT, your todo list assistant. Would you like me to add a new task to your todo list?");
    }

    private sealed class CustomInteractions
    {
        private readonly List<TodoItem> _todoList = [];

        [KernelFunction, Description("Gets today's date.")]
        public DateTime GetDate() => DateTime.Today;

        [KernelFunction, Description("Adds a new todo item to the user's todo list.")]
        public string AddTodoItem([Description("The todo item to add to the user's todo list.")] TodoItem item)
        {
            _todoList.Add(item);
            return "Item added to the todo list.";
        }

        [KernelFunction, Description("Completes (and therefore deletes) an item on the user's todo list.")]
        public string MarkItemAsComplete([Description("The id of the todo item to mark as complete.")] Guid todoItemId)
        {
            var numberRemoved = _todoList.RemoveAll(x => x.Id == todoItemId);
            return "Marked todo item as complete. This item has now been removed from the user's todo list.";
        }

        [KernelFunction, Description("Gets the items on the user's todo list.")]
        public string GetTodoItems()
        {
            return JsonSerializer.Serialize(_todoList);
        }

        [KernelFunction, Description("Modifies an existing todo item on the user's todo list.")]
        public string UpdateTodoItem([Description("The updated todo item. The todo item ID must match the ID of the item to update.")] TodoItem updatedItem)
        {
            var currentItem = _todoList.SingleOrDefault(i => i.Id == updatedItem.Id);

            if (currentItem is null)
            {
                return $"Did not find a todo item with the id {updatedItem.Id}.";
            }

            var index = _todoList.IndexOf(currentItem);
            _todoList[index] = updatedItem;

            return "Todo item was updated.";
        }
    }
}
