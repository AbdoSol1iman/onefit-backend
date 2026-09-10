using System.Text.Json;
using OneFit.Api.Endpoints;
using OneFit.Api.EndPoints.AuthEndPoints;
using OneFit.Api.EndPoints.Cart;
using OneFit.Api.EndPoints.WishList;
using OneFit.Infrastructure;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    o.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
});
builder.Services.AddAuthorization();

var app = builder.Build();
await app.Services.SeedIdentityAsync();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapCatalog();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
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

//wishlist endpoints
app.MapWishlistEndpoints();
//cart endpoints
app.MapCartEndpoints();

//auth endpoints
app.MapLoginEndPoint();
app.MapRegisterBrandEndPoint();
app.MapRegisterUserEndPoint();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
