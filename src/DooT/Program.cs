using DooT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddTransient<Chatbot>();
builder.Services.AddTransient<Application>();

var aiOptions = builder.Configuration.GetSection("AI").Get<AIOptions>()
    ?? throw new InvalidOperationException("Missing AI configuration section.");

builder.Services.AddKernel().AddOpenAIChatCompletion(aiOptions.ModelId, aiOptions.ApiKey);
var host = builder.Build();

await host.Services.GetRequiredService<Application>().Run();
