using System.Text.Json;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using OneFit.Api.Endpoints;
using OneFit.Api.Endpoints.AuthEndPoints;
using OneFit.Api.Endpoints.Cart;
using OneFit.Api.Endpoints.Checkouts;
using OneFit.Api.Endpoints.Orders;
using OneFit.Api.Endpoints.Payments;
using OneFit.Api.Endpoints.Feed;
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

// --- Rate limiting (Critical fix #9) ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global sliding window: 100 requests per 60 s per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetSlidingWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(60),
                SegmentsPerWindow = 4,
                QueueLimit = 0
            }));

    // Stricter policy for auth endpoints: 10 per 60 s
    options.AddPolicy("auth", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(60),
                QueueLimit = 0
            }));

    // AI stylist: 20 per 60 s
    options.AddPolicy("stylist", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromSeconds(60),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// --- Global exception handler (Critical fix #10) ---
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = "One or more validation errors occurred.",
                        details = validationEx.Errors.Select(e => new
                        {
                            field = e.PropertyName,
                            message = e.ErrorMessage
                        })
                    }
                });
                break;

            case KeyNotFoundException:
            case OneFit.Application.Common.Exceptions.NotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "NOT_FOUND",
                        message = exception.Message
                    }
                });
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "UNAUTHORIZED",
                        message = "Authentication is required."
                    }
                });
                break;

            case InvalidOperationException when exception.Message.Contains("stock", StringComparison.OrdinalIgnoreCase):
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "INSUFFICIENT_STOCK",
                        message = exception.Message
                    }
                });
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                // Never leak internal details to clients
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An unexpected error occurred. Please try again later."
                    }
                });
                break;
        }
    });
});

// Critical fix #5: Use the restrictive AllowFrontend policy, NOT AllowAll
app.UseCors("AllowFrontend");

app.UseRateLimiter();

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

app.UseHttpsRedirection();

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
app.MapStylist();
app.MapFeedEndpoints();
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

app.Run();
