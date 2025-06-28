using Azure;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Serilog;
using Sleepr.Agents;
using Sleepr.Interfaces;
using Sleepr.Mail;
using Sleepr.Mail.Interfaces;
using Sleepr.Plugins;
using Sleepr.Services;
using System.Text.Json;
using System.Text.Json.Serialization;


// The 'Port syncing with extension' chat details transitioning to a system tray application with a web interface.

DotNetEnv.Env.Load();

// Ensure the DotNetEnv NuGet package is installed in your project. You can install it using the following command in the terminal:
// dotnet add package DotNetEnv
var key = Environment.GetEnvironmentVariable("AZURE_MISTRAL_NEMO_KEY") ?? throw new InvalidOperationException("AZURE_MISTRAL_NEMO_KEY environment variable is not set.");
var credential = new AzureKeyCredential(key);

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Model: {Environment.GetEnvironmentVariable("AZURE_MODEL_ID")}");

// Add services to the container.

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
);
builder.Logging.ClearProviders();

builder.Services.AddScoped<McpPluginManager>(serviceProvider =>
     {
         // 1. Load all manifests from the "test-plugins" folder
         var pluginFolder = Path.Combine(Directory.GetCurrentDirectory(), "Plugins/MCPServers");
         var manifests = PluginLoader.LoadManifests(pluginFolder);
         var logger = serviceProvider.GetRequiredService<ILogger<McpPluginManager>>();
         // 2. Construct and return the manager
         return new McpPluginManager(manifests, logger);
     });

// builder.Services.AddSingleton<IPromptLoader>(new YamlPromptLoader(Serilog.ILogger<YamlPromptLoader>, "prompts"));
builder.Services.AddSingleton<IPromptLoader>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<YamlPromptLoader>>();
    return new YamlPromptLoader(logger, "prompts");
});
builder.Services.AddSingleton<IPromptTemplateFactory, KernelPromptTemplateFactory>();
builder.Services.AddScoped<IAgentRunner, SleeprAgentRunner>();
builder.Services.AddScoped<IChatCompletionsRunner, ChatCompletionsRunner>();
builder.Services.AddScoped<ISleeprAgentFactory, SleeprAgentFactory>();
builder.Services.AddRazorPages();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
          // *exactly* your extension origin, no "@temporary-addon"
          .WithOrigins(Environment.GetEnvironmentVariable("CORS_EXCEMPTION")!)
          .AllowAnyHeader()
          .AllowAnyMethod();
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions => listenOptions.UseHttps());
});

#pragma warning disable SKEXP0070
builder.Services.AddAzureAIInferenceChatCompletion(
    endpoint: new Uri(Environment.GetEnvironmentVariable("AZURE_ENDPOINT")!),
    modelId: Environment.GetEnvironmentVariable("AZURE_MODEL_ID")!,
    apiKey: key
);

builder.Services.AddTransient((serviceProvider) =>
{
    return new Kernel(serviceProvider);
});

// Configure agent output to use SQLite database
var outputDbPath = builder.Configuration["OutputDb:Path"] ?? "agent-output.db";
var connectionString = Environment.GetEnvironmentVariable("OUTPUT_DB_CONNECTION_STRING")
    ?? $"Data Source={Path.Combine(Directory.GetCurrentDirectory(), outputDbPath)}";

builder.Services.AddSingleton(new DbOutputOptions { ConnectionString = connectionString });
builder.Services.AddScoped<IAgentOutput, DbAgentOutput>();

var username = Environment.GetEnvironmentVariable("FASTMAIL_USERNAME");
var password = Environment.GetEnvironmentVariable("FASTMAIL_APP_PASSWORD");
if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
{
    builder.Services.AddScoped<FastmailImapHandler>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<FastmailImapHandler>>();
        return new FastmailImapHandler(logger, username, password);
    });
}

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(
            JsonNamingPolicy.CamelCase, // if you want "system" vs. "System"
            allowIntegerValues: false   // safer: disallow numeric enums
        ));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

app.MapGet("/api/status", () => "Sleepr is running!");
app.MapGet("/test-email", async (FastmailImapHandler? emailReader) =>
{
    if (emailReader == null)
        return Results.Problem("Email reader not available.");

    var emails = await emailReader.FetchRecentAsync();
    return Results.Ok(emails.Select(e => new { e.Subject, e.From , e.Body}));
});

app.Run();