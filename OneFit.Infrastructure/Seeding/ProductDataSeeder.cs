using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Seeding;

/// <summary>
/// Reads the generated products.json file and seeds Brands + Products
/// into the database. Safe to run multiple times (idempotent on brand name
/// and product name+brand, so re-running won't duplicate everything).
/// </summary>
public static class ProductDataSeeder
{
    private sealed class SeedProductDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = null!;

        [JsonPropertyName("brand")]
        public string Brand { get; set; } = null!;

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        [JsonPropertyName("subCategory")]
        public string SubCategory { get; set; } = null!;

        [JsonPropertyName("articleType")]
        public string ArticleType { get; set; } = null!;

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = null!;

        [JsonPropertyName("baseColour")]
        public string BaseColour { get; set; } = null!;

        [JsonPropertyName("season")]
        public string Season { get; set; } = null!;

        [JsonPropertyName("usage")]
        public string Usage { get; set; } = null!;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = null!;
    }

    /// <param name="context">Your DbContext (e.g. OneFitDbContext).</param>
    /// <param name="jsonFilePath">Full path to products.json.</param>
    /// <param name="menswearOnly">
    /// If true, only rows whose real "gender" field is Men or Unisex are seeded.
    /// Fix the Python script first so this field reflects the dataset's actual
    /// gender column, not an index-based guess.
    /// </param>
    public static async Task SeedAsync(DbContext context, string jsonFilePath, bool menswearOnly = true)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"Seed file not found: {jsonFilePath}");

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawProducts = JsonSerializer.Deserialize<List<SeedProductDto>>(json, jsonOptions) ?? new();

        if (menswearOnly)
        {
            rawProducts = rawProducts
                .Where(p => p.Gender.Equals("Men", StringComparison.OrdinalIgnoreCase)
                         || p.Gender.Equals("Unisex", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (rawProducts.Count == 0)
        {
            Console.WriteLine("No products matched the filter — nothing to seed.");
            return;
        }

        // 1) Upsert brands first, so products can reference a real BrandId.
        var brandNames = rawProducts.Select(p => p.Brand).Distinct().ToList();

        var existingBrands = await context.Set<Brand>()
            .Where(b => brandNames.Contains(b.Name))
            .ToDictionaryAsync(b => b.Name);

        foreach (var name in brandNames)
        {
            if (!existingBrands.ContainsKey(name))
            {
                var brand = new Brand
                {
                    BrandId = Guid.NewGuid().ToString(),
                    Name = name
                };
                context.Set<Brand>().Add(brand);
                existingBrands[name] = brand;
            }
        }

        await context.SaveChangesAsync();

        // 2) Skip products that already exist (same name + brand) so re-runs are safe.
        var existingProductKeys = await context.Set<Product>()
            .Select(p => new { p.Name, p.BrandId })
            .ToListAsync();

        var existingKeySet = existingProductKeys
            .Select(k => (k.Name, k.BrandId))
            .ToHashSet();

        var newProducts = new List<Product>();

        foreach (var dto in rawProducts)
        {
            var brand = existingBrands[dto.Brand];

            if (existingKeySet.Contains((dto.ProductName, brand.BrandId)))
                continue; // already seeded

            var product = new Product
            {
                ProductId = Guid.NewGuid().ToString(),
                BrandId = brand.BrandId,
                Name = dto.ProductName,
                Category = string.IsNullOrWhiteSpace(dto.ArticleType) ? dto.Category : dto.ArticleType,
                PriceEgp = dto.Price,
                ImageUrl = dto.ImageUrl,
                StyleTags = new List<string> { dto.Usage, dto.Season, dto.BaseColour, dto.SubCategory }
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                CreatedAt = DateTime.UtcNow
            };

            newProducts.Add(product);
        }

        if (newProducts.Count > 0)
        {
            context.Set<Product>().AddRange(newProducts);
            await context.SaveChangesAsync();
        }

        Console.WriteLine($"Seeded {existingBrands.Count} brand(s) and {newProducts.Count} new product(s) " +
                          $"({rawProducts.Count - newProducts.Count} already existed).");
    }
}