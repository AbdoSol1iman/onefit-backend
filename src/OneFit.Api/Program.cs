using System.Text.Json;
using OneFit.Api.Endpoints;
using OneFit.Api.Endpoints.AuthEndPoints;
using OneFit.Api.Endpoints.Cart;
using OneFit.Api.Endpoints.Checkouts;
using OneFit.Api.Endpoints.Orders;
using OneFit.Api.Endpoints.Payments;
using OneFit.Api.Endpoints.Feed;
using OneFit.Api.Endpoints.WishList;
using OneFit.Infrastructure;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Seeding;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructureServices(builder.Configuration);
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
