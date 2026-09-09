using System.ComponentModel.DataAnnotations;

namespace OneFit.Api.Endpoints;

/// <summary>
/// Request model for querying products in the catalog
/// </summary>
public record CatalogQueryRequest(
    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, MinimumLength = 1)]
    string Category,

    [Range(0, double.MaxValue, ErrorMessage = "MaxPriceEgp must be a positive number")]
    decimal? MaxPriceEgp,

    List<string>? StyleTags,

    bool InStockOnly,

    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
    int Limit = 10);
