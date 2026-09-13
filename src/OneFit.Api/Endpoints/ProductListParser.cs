using OneFit.Application.Features.Products;

namespace OneFit.Api.Endpoints;

internal static class ProductListParser
{
    private static readonly HashSet<string> Sorts = ["price_asc", "price_desc", "newest"];
    private static readonly HashSet<string> StyleMatches = ["rank", "all", "any"];

    internal sealed record ParseResult(ProductListQuery? Query, string? Error);

    internal static ParseResult TryParse(
        string? category,
        decimal? maxPriceEgp,
        string? search,
        bool? inStockOnly,
        string? sort,
        int? page,
        int? pageSize,
        string? styleTags = null,
        string? styleMatch = null,
        int? limit = null)
    {
        sort = string.IsNullOrWhiteSpace(sort) ? "price_asc" : sort.Trim().ToLower();
        if (!Sorts.Contains(sort))
            return Fail("sort must be price_asc, price_desc or newest");

        var styleMatchValue = string.IsNullOrWhiteSpace(styleMatch) ? "rank" : styleMatch.Trim().ToLower();
        if (!StyleMatches.Contains(styleMatchValue))
            return Fail("style_match must be rank, all or any");

        var tags = ParseTags(styleTags);

        if (limit is < 1 or > 20)
            return Fail("limit must be 1..20");

        var pageValue = page ?? 1;
        if (pageValue < 1)
            return Fail("page must be >= 1");

        var pageSizeValue = pageSize ?? 20;
        if (pageSizeValue is < 1 or > 50)
            return Fail("page_size must be 1..50");

        if (maxPriceEgp is < 0)
            return Fail("max_price_egp must be >= 0");

        return new ParseResult(
            new ProductListQuery(category, maxPriceEgp, search, inStockOnly ?? true, sort, pageValue, pageSizeValue, tags, styleMatchValue, limit),
            Error: null);
    }

    internal static List<string>? ParseTags(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var tags = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.ToLower())
            .Distinct()
            .ToList();
        return tags.Count == 0 ? null : tags;
    }

    private static ParseResult Fail(string error) => new(Query: null, Error: error);
}
