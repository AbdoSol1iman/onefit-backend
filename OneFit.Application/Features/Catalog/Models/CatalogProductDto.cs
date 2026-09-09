using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OneFit.Application.Features.Catalog.Models
{
    public record CatalogProductDto(
    string ProductId,
    string Name,
    string BrandId,
    string BrandName,
    string Category,
    decimal PriceEgp,
    string? ImageUrl,
    IReadOnlyList<string> StyleTags,
    IReadOnlyList<SizeStockDto> SizesInStock,
    DateTime? CreatedAt);
}
