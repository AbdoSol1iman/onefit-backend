using OneFit.Application.Features.Products;

namespace OneFit.Api.Endpoints;

internal static class ProductListParser
{
    private static readonly HashSet<string> Sorts = ["price_asc", "price_desc", "newest"];

    internal sealed record ParseResult(ProductListQuery? Query, string? Error);

    internal static ParseResult TryParse(
        string? category,
        decimal? maxPriceEgp,
        string? search,
        bool? inStockOnly,
        string? sort,
        int? page,
        int? pageSize)
    {
        sort = string.IsNullOrWhiteSpace(sort) ? "price_asc" : sort.Trim().ToLower();
        if (!Sorts.Contains(sort))
            return Fail("sort must be price_asc, price_desc or newest");

        var pageValue = page ?? 1;
        if (pageValue < 1)
            return Fail("page must be >= 1");

        var pageSizeValue = pageSize ?? 20;
        if (pageSizeValue is < 1 or > 50)
            return Fail("page_size must be 1..50");

        if (maxPriceEgp is < 0)
            return Fail("max_price_egp must be >= 0");

        return new ParseResult(
            new ProductListQuery(category, maxPriceEgp, search, inStockOnly ?? true, sort, pageValue, pageSizeValue),
            Error: null);
    }

    private static ParseResult Fail(string error) => new(Query: null, Error: error);
}
