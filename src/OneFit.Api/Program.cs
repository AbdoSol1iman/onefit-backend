using System.Text.Json;
using OneFit.Api.Endpoints;
using OneFit.Api.EndPoints.Cart;
using OneFit.Api.EndPoints.WishList;
using OneFit.Application.Features.Chatbot;
using OneFit.Infrastructure;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Seeding;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.Configure<ChatbotOptions>(
    builder.Configuration.GetSection(ChatbotOptions.SectionName));
builder.Services.AddHttpClient<IChatbotClient, ChatbotClient>((sp, http) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ChatbotOptions>>().Value;
    var baseUrl = (string.IsNullOrWhiteSpace(options.BaseUrl) ? "http://127.0.0.1:8000" : options.BaseUrl).TrimEnd('/');
    http.BaseAddress = new Uri(baseUrl + "/");
    http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds);
});
builder.Services.AddScoped<IChatbotOrchestrator, ChatbotOrchestrator>();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    o.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()?
    .Where(o => !string.IsNullOrWhiteSpace(o))
    .Append("http://localhost:5173")
    .Distinct()
    .ToArray() ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ApiDocs:Enabled"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health")
    .WithSummary("Liveness probe for Azure health checks and frontend ping.");

app.UseHttpsRedirection();

if (args.Contains("--seed"))
{
    Console.WriteLine("SEED MODE STARTED!");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OneFitDbContext>();

    var jsonPath = Path.Combine(
        AppContext.BaseDirectory,
        "Seeding",
        "Data",
        "products.json"
    );

    Console.WriteLine($"JSON PATH: {jsonPath}");
    Console.WriteLine($"FILE EXISTS: {File.Exists(jsonPath)}");

    await ProductDataSeeder.SeedAsync(db, jsonPath);

    Console.WriteLine("SEED DONE!");
    return;
}

app.MapProducts();
app.MapStylist();
app.MapChatbot();
app.MapWishlistEndpoints();
app.MapCartEndpoints();

app.Run();
