using System.Text.Json;
using OneFit.Api.Endpoints;
using OneFit.Api.EndPoints.Cart;
using OneFit.Api.EndPoints.WishList;
using OneFit.Infrastructure;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    o.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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

app.MapCatalog();
app.MapProducts();
app.MapWishlistEndpoints();
app.MapCartEndpoints();

app.Run();
