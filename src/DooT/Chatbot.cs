using BAI.ConsoleApp;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        private readonly List<TodoItem> _todoList = [];

        [KernelFunction, Description("Gets the date that is a specified number of days from today.")]
        public static DateTime GetRelativeDate([Description("The number of days to add, relative to today.")] int daysToAdd) => DateTime.Today.AddDays(daysToAdd);

        [KernelFunction, Description("Adds a new todo item to the user's todo list.")]
        public string AddTodoItem(
            [Description("The todo item description.")] string description,
            [Description("The due date of the todo item (optional).")] DateTime? dueDate = null,
            [Description("The priority of the todo item (optional).")] string? priority = null)
        {
            var todoItem = new TodoItem
            {
                Id = _todoList.Count == 0 ? 1 : _todoList.Max(x => x.Id) + 1,
                Description = description,
                DueDate = dueDate,
                Priority = priority
            };

            _todoList.Add(todoItem);

            return $"The following todo item was successfully added to the user's todo list:\n{JsonSerializer.Serialize(todoItem, _serializerOptions)}";
        }

        [KernelFunction, Description("Completes (and therefore deletes) an item on the user's todo list.")]
        public string MarkItemAsComplete([Description("The id of the todo item to mark as complete.")] int todoItemId)
        {
            var numberRemoved = _todoList.RemoveAll(x => x.Id == todoItemId);
            return "Marked todo item as complete. This item has now been removed from the user's todo list.";
        }

        [KernelFunction, Description("Gets the items on the user's todo list.")]
        public string GetTodoItems()
        {
            return JsonSerializer.Serialize(_todoList, _serializerOptions);
        }

        [KernelFunction, Description("Updates the description of a todo item on the user's todo list.")]
        public string UpdateDescription([Description("The id of the todo item to update.")] int todoItemId, [Description("The new description.")] string newDescription)
        {
            var item = _todoList.SingleOrDefault(x => x.Id == todoItemId);

            if (item is null)
                return $"Did not find a todo item with the id {todoItemId}.";

            item.Description = newDescription;
            return "Todo item description was updated.";
        }

        [KernelFunction, Description("Updates the due date of a todo item on the user's todo list.")]
        public string UpdateDueDate([Description("The id of the todo item to update.")] int todoItemId, [Description("The new due date.")] DateTime newDueDate)
        {
            var item = _todoList.SingleOrDefault(x => x.Id == todoItemId);

            if (item is null)
                return $"Did not find a todo item with the id {todoItemId}.";

            item.DueDate = newDueDate;
            return "Todo item due date was updated.";
        }

        [KernelFunction, Description("Updates the priority of a todo item on the user's todo list.")]
        public string UpdatePriority([Description("The id of the todo item to update.")] int todoItemId, [Description("The new priority.")] string newPriority)
        {
            var item = _todoList.SingleOrDefault(x => x.Id == todoItemId);

            if (item is null)
                return $"Did not find a todo item with the id {todoItemId}.";

            item.Priority = newPriority;
            return "Todo item priority was updated.";
        }
    }
}
