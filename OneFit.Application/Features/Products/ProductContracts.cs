namespace OneFit.Application.Features.Products;

public sealed record ProductListQuery(
    string? Category,
    decimal? MaxPriceEgp,
    string? Search,
    bool InStockOnly = true,
    string Sort = "price_asc",
    int Page = 1,
    int PageSize = 20,
    List<string>? StyleTags = null,
    string StyleMatch = "rank",
    int? Limit = null
);

public sealed record ProductSummaryDto(
    string ProductId,
    string Brand,
    string Name,
    string Category,
    decimal PriceEgp,
    List<string> SizesInStock,
    string? ImageUrl
);

public sealed record ProductSizeDto(string Size, int StockQty);

public sealed record ProductDetailDto(
    string ProductId,
    string Brand,
    string BrandId,
    string Name,
    string Category,
    decimal PriceEgp,
    List<string>? StyleTags,
    List<ProductSizeDto> Sizes,
    string? ImageUrl
);

public sealed record PagedResult<T>(List<T> Items, int Page, int PageSize, int Total);

public interface IProductQueryService
{
    Task<PagedResult<ProductSummaryDto>> ListAsync(ProductListQuery query, CancellationToken ct = default);
    Task<ProductDetailDto?> GetByIdAsync(string productId, CancellationToken ct = default);
}
