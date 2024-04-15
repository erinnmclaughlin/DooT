using DooT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddTransient<Chatbot>();
builder.Services.AddTransient<Application>();

builder.Services.Configure<AIOptions>(builder.Configuration.GetSection("AI"));

var aiOptions = builder.Configuration.GetSection("AI").Get<AIOptions>();

if (aiOptions?.ApiKey is { Length: > 0 } apiKey && aiOptions?.ModelId is { Length: > 0 } model)
{
    builder.Services.AddKernel().AddOpenAIChatCompletion(model, apiKey);
}

var host = builder.Build();

await host.Services.GetRequiredService<Application>().Run();
