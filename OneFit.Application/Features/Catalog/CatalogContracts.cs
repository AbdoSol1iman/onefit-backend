namespace OneFit.Application.Features.Catalog;

public sealed record QueryCatalogRequest(
    string? Category,
    decimal? MaxPriceEgp,
    List<string>? StyleTags,
    bool InStockOnly = true,
    int Limit = 3
);

public sealed record CatalogItemDto(
    string ProductId,
    string Brand,
    string Name,
    decimal PriceEgp,
    List<string> SizesInStock,
    string? ImageUrl
);

public sealed record QueryCatalogResponse(List<CatalogItemDto> Results);

public interface ICatalogQueryService
{
    Task<QueryCatalogResponse> QueryAsync(QueryCatalogRequest request, CancellationToken ct = default);
}
