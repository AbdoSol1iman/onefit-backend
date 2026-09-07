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

    public static async Task SeedAsync(
        DbContext context,
        string jsonFilePath,
        bool menswearOnly = true)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException(
                $"Seed file not found: {jsonFilePath}");

        var json = await File.ReadAllTextAsync(jsonFilePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var rawProducts =
            JsonSerializer.Deserialize<List<SeedProductDto>>(json, options)
            ?? new List<SeedProductDto>();

        // ============================================
        // 1. Filter Men's / Unisex products
        // ============================================

        if (menswearOnly)
        {
            rawProducts = rawProducts
                .Where(p =>
                    p.Gender.Equals(
                        "Men",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    p.Gender.Equals(
                        "Unisex",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (rawProducts.Count == 0)
        {
            Console.WriteLine(
                "No products matched the filter — nothing to seed.");

            return;
        }

        // ============================================
        // 2. Upsert Brands
        // ============================================

        var brandNames = rawProducts
            .Where(p => !string.IsNullOrWhiteSpace(p.Brand))
            .Select(p => p.Brand.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingBrands = await context.Set<Brand>()
            .Where(b => brandNames.Contains(b.Name))
            .ToListAsync();

        var brandDictionary = existingBrands
            .ToDictionary(
                b => b.Name,
                b => b,
                StringComparer.OrdinalIgnoreCase);

        foreach (var brandName in brandNames)
        {
            if (!brandDictionary.ContainsKey(brandName))
            {
                var brand = new Brand
                {
                    BrandId = Guid.NewGuid().ToString(),
                    Name = brandName
                };

                context.Set<Brand>().Add(brand);

                brandDictionary[brandName] = brand;
            }
        }

        await context.SaveChangesAsync();

        // ============================================
        // 3. Load existing products
        // ============================================

        var existingProducts = await context.Set<Product>()
            .Include(p => p.ProductSizes)
            .ToListAsync();

        var existingProductDictionary = existingProducts
            .ToDictionary(
                p => $"{p.BrandId}|{p.Name.Trim().ToLowerInvariant()}",
                p => p);

        int addedProducts = 0;
        int updatedProducts = 0;
        int addedSizes = 0;

        // ============================================
        // 4. Upsert Products
        // ============================================

        foreach (var dto in rawProducts)
        {
            if (string.IsNullOrWhiteSpace(dto.ProductName) ||
                string.IsNullOrWhiteSpace(dto.Brand))
            {
                continue;
            }

            var brandName = dto.Brand.Trim();

            if (!brandDictionary.TryGetValue(
                    brandName,
                    out var brand))
            {
                continue;
            }

            var productKey =
                $"{brand.BrandId}|{dto.ProductName.Trim().ToLowerInvariant()}";

            // --------------------------------------------
            // Category mapping
            // --------------------------------------------

            var category = MapCategory(
                dto.ArticleType,
                dto.SubCategory,
                dto.Category);

            // --------------------------------------------
            // Style tags
            // --------------------------------------------

            var styleTags = BuildStyleTags(dto);

            // --------------------------------------------
            // Existing product -> UPDATE
            // --------------------------------------------

            if (existingProductDictionary.TryGetValue(
                    productKey,
                    out var existingProduct))
            {
                existingProduct.Category = category;
                existingProduct.PriceEgp = dto.Price;
                existingProduct.ImageUrl = dto.ImageUrl;
                existingProduct.StyleTags = styleTags;

                if (existingProduct.CreatedAt == null)
                    existingProduct.CreatedAt = DateTime.UtcNow;

                updatedProducts++;

                // Repair missing sizes
                addedSizes += await EnsureProductSizesAsync(
                    context,
                    existingProduct);
            }

            // --------------------------------------------
            // New product -> INSERT
            // --------------------------------------------

            else
            {
                var product = new Product
                {
                    ProductId = Guid.NewGuid().ToString(),
                    BrandId = brand.BrandId,
                    Name = dto.ProductName.Trim(),
                    Category = category,
                    PriceEgp = dto.Price,
                    ImageUrl = dto.ImageUrl,
                    StyleTags = styleTags,
                    CreatedAt = DateTime.UtcNow
                };

                context.Set<Product>().Add(product);

                // Add sizes to new product
                var sizes = GetSizesForCategory(category);

                foreach (var size in sizes)
                {
                    product.ProductSizes.Add(
                        new ProductSize
                        {
                            ProductId = product.ProductId,
                            Size = size,
                            StockQty = 10
                        });

                    addedSizes++;
                }

                existingProductDictionary[productKey] = product;

                addedProducts++;
            }
        }

        await context.SaveChangesAsync();

        Console.WriteLine(
            $"Product seeding completed. " +
            $"Added: {addedProducts}, " +
            $"Updated: {updatedProducts}, " +
            $"Sizes added: {addedSizes}, " +
            $"Total source products: {rawProducts.Count}");
    }

    // ========================================================
    // CATEGORY MAPPING
    // ========================================================

    private static string MapCategory(
        string? articleType,
        string? subCategory,
        string? category)
    {
        var value =
            $"{articleType} {subCategory} {category}"
                .ToLowerInvariant();

        // Shirts / T-shirts / Tops
        if (value.Contains("shirt") ||
            value.Contains("t-shirt") ||
            value.Contains("tee") ||
            value.Contains("top") ||
            value.Contains("kurta") ||
            value.Contains("sweater") ||
            value.Contains("hoodie") ||
            value.Contains("jacket") ||
            value.Contains("coat"))
        {
            return "shirt";
        }

        // Pants / Jeans / Trousers / Shorts
        if (value.Contains("pant") ||
            value.Contains("jean") ||
            value.Contains("trouser") ||
            value.Contains("short") ||
            value.Contains("track"))
        {
            return "pants";
        }

        // Shoes
        if (value.Contains("shoe") ||
            value.Contains("sandal") ||
            value.Contains("sneaker") ||
            value.Contains("loafer") ||
            value.Contains("boot") ||
            value.Contains("flip flop"))
        {
            return "shoes";
        }

        // Everything else
        return "accessory";
    }

    // ========================================================
    // STYLE TAGS
    // ========================================================

    private static List<string> BuildStyleTags(
        SeedProductDto dto)
    {
        var tags = new List<string>();

        AddTag(tags, dto.Usage);
        AddTag(tags, dto.Season);
        AddTag(tags, dto.BaseColour);
        AddTag(tags, dto.SubCategory);

        return tags
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(t => t.ToLowerInvariant())
            .ToList();
    }

    private static void AddTag(
        List<string> tags,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            tags.Add(value.Trim());
        }
    }

    // ========================================================
    // PRODUCT SIZES
    // ========================================================

    private static string[] GetSizesForCategory(
        string category)
    {
        return category switch
        {
            "shirt" =>
                new[] { "S", "M", "L", "XL" },

            "pants" =>
                new[] { "30", "32", "34", "36" },

            "shoes" =>
                new[] { "40", "41", "42", "43", "44" },

            "accessory" =>
                new[] { "OneSize" },

            _ =>
                new[] { "OneSize" }
        };
    }

    // ========================================================
    // REPAIR EXISTING PRODUCT SIZES
    // ========================================================

    private static async Task<int> EnsureProductSizesAsync(
        DbContext context,
        Product product)
    {
        var requiredSizes =
            GetSizesForCategory(product.Category);

        var existingSizes =
            product.ProductSizes
                .Select(s => s.Size)
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        int added = 0;

        foreach (var size in requiredSizes)
        {
            if (existingSizes.Contains(size))
                continue;

            context.Set<ProductSize>().Add(
                new ProductSize
                {
                    ProductId = product.ProductId,
                    Size = size,
                    StockQty = 10
                });

            added++;
        }

        return added;
    }
}