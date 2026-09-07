
namespace OneFit.Api.Endpoints;

public record CatalogQueryRequest(
    string Category,
    decimal? MaxPriceEgp,
    List<string>? StyleTags,
    bool InStockOnly,
    int Limit = 3);