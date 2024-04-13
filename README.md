# DooT

A sample project demonstrating how to build an AI-powered application using [Semantic Kernel](https://github.com/microsoft/semantic-kernel) and [OpenAI API](https://platform.openai.com/docs/introduction).

<img width="854" alt="image" src="https://github.com/erinnmclaughlin/DooT/assets/22223146/688ebdd6-f37b-4d49-b9a4-ee866e237b53">

## How it Works
DooT manages the user's ToDo list by invoking methods it has been given access to within the code. For example, DooT may invoke the `AddToDoItem` method when the user asks to add an item to their list:
```cs
[KernelFunction, Description("Adds a new todo item to the user's todo list.")]
public string AddTodoItem(
    [Description("The todo item description.")] string description,
    [Description("The due date of the todo item (optional).")] DateTime? dueDate = null,
    [Description("The priority of the todo item (optional).")] string? priority = null)
{
    // code to add the todo item

    return $"The following todo item was successfully added to the user's todo list:\n{JsonSerializer.Serialize(todoItem, _serializerOptions)}";
}
```

## Getting Started
You will need to update the `appsettings.json` file with your own [API key](https://platform.openai.com/api-keys) from OpenAI.
