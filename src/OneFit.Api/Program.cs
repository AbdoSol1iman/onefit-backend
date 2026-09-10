using OneFit.Api.Endpoints;
using OneFit.Api.EndPoints.AdminEndPoints;
using OneFit.Api.EndPoints.AuthEndPoints;
using OneFit.Api.EndPoints.Cart;
using OneFit.Api.EndPoints.WishList;
using OneFit.Infrastructure;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Seeding;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("========== DB CONNECTION ==========");
Console.WriteLine(cs);
Console.WriteLine("==================================");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddAuthorization();

var app = builder.Build();

await app.Services.SeedIdentityAsync();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Catalog endpoints
app.MapCatalogEndpoints();

// Wishlist endpoints
app.MapWishlistEndpoints();

// Cart endpoints
app.MapCartEndpoints();

// Auth endpoints
app.MapLoginEndPoint();
app.MapRegisterBrandEndPoint();
app.MapRegisterUserEndPoint();
//brand endpoints


var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5)
        .Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

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

app.Run();

record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF =>
        32 + (int)(TemperatureC / 0.5556);
}