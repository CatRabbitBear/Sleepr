using Azure;
using DotNetEnv;
using Microsoft.SemanticKernel;
using Sleepr.Interfaces;
using Sleepr.Mail;
using Sleepr.Mail.Interfaces;
using Sleepr.Services;
using System.Text.Json;
using System.Text.Json.Serialization;


// Ensure the DotNetEnv NuGet package is installed in your project. You can install it using the following command in the terminal:
// dotnet add package DotNetEnv

DotNetEnv.Env.Load();
// Add this at the top of the file with the other using directives

// Ensure the DotNetEnv NuGet package is installed in your project. You can install it using the following command in the terminal:
// dotnet add package DotNetEnv
var key = Environment.GetEnvironmentVariable("AZURE_MISTRAL_NEMO_KEY") ?? throw new InvalidOperationException("AZURE_MISTRAL_NEMO_KEY environment variable is not set.");
var credential = new AzureKeyCredential(key);

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Model: {Environment.GetEnvironmentVariable("AZURE_MODEL_ID")}");

// Add services to the container.
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

builder.Services.AddScoped<IAgentOutput, FileAgentOutput>(serviceProvider =>
{
    // Use a directory in the current working directory for output files
    var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "agent-output");
    Directory.CreateDirectory(outputDirectory);
    return new FileAgentOutput(outputDirectory);
});

var username = Environment.GetEnvironmentVariable("FASTMAIL_USERNAME");
var password = Environment.GetEnvironmentVariable("FASTMAIL_APP_PASSWORD");
if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
{
    builder.Services.AddScoped<FastmailImapHandler>(sp =>
    {
        return new FastmailImapHandler(username, password);
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
//app.MapPost("/api/run-agent-task", async (HttpRequest req) =>
//{
//    // Read the entire JSON payload
//    using var reader = new StreamReader(req.Body);
//    var body = await reader.ReadToEndAsync();

//    // Log it to the console
//    Console.WriteLine("Received /api/run-agent-task:");
//    Console.WriteLine(body);

//    // Echo back a simple acknowledgment
//    return Results.Ok(new { status = "received", payload = body });
//});

app.Run();