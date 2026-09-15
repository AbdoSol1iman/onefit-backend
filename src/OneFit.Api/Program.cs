using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OneFit.Api.Endpoints;
using OneFit.Api.Endpoints.AuthEndPoints;
using OneFit.Api.Endpoints.Cart;
using OneFit.Api.Endpoints.Checkouts;
using OneFit.Api.Endpoints.Orders;
using OneFit.Api.Endpoints.Payments;
using OneFit.Api.Endpoints.WishList;
using OneFit.Application.Features.Chatbot;
using OneFit.Infrastructure;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Seeding;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
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

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OneFitDbContext>();
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}. Applying fallback SQL...");
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE EXTENSION IF NOT EXISTS vector;
            DO $$ BEGIN
                ALTER TABLE products ADD COLUMN image_embedding vector(512);
            EXCEPTION WHEN duplicate_column THEN NULL;
            END $$;
            DO $$ BEGIN
                CREATE INDEX IF NOT EXISTS idx_products_image_embedding ON products USING hnsw (image_embedding vector_cosine_ops);
            EXCEPTION WHEN undefined_table THEN NULL;
            END $$;";
        await cmd.ExecuteNonQueryAsync();
        await conn.CloseAsync();
    }
}

await app.Services.SeedIdentityAsync();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ApiDocs:Enabled"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health")
    .WithSummary("Liveness probe for Azure health checks and frontend ping.");

if (args.Contains("--seed"))
{
    Console.WriteLine("SEED MODE STARTED!");

    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<OneFitDbContext>();

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
app.MapChatbot();
app.MapWishlistEndpoints();
app.MapCartEndpoints();
app.MapGetCartEndPoint();
app.MapCheckoutEndPoint();
app.MapGetOrdersEndPoint();
app.MapLoginEndPoint();
app.MapRegisterBrandEndPoint();
app.MapRegisterUserEndPoint();
app.MapStripeWebhookEndpoint();
app.MapVisualSearch();

app.Run();
