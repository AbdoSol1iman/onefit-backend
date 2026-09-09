
using MediatR;
using OneFit.Application.Common.Interfaces.Catalog;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.Catalog.Models;
using OneFit.Domain.Entities;

namespace OneFit.Application.Features.Catalog.Queries.QueryCatalog;

public class QueryCatalogQueryHandler : IRequestHandler<QueryCatalogQuery, CatalogQueryResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public QueryCatalogQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<CatalogQueryResponseDto> Handle(QueryCatalogQuery request, CancellationToken ct)
    {
        var filter = new CatalogFilter(
            Category: request.Category,
            MaxPriceEgp: request.MaxPriceEgp,
            StyleTags: request.StyleTags,
            InStockOnly: request.InStockOnly,
            Limit: request.Limit);

        var products = await _unitOfWork.Products.QueryAsync(filter, ct);

       
        var results = products.Select(MapToDto).ToList();

        return new CatalogQueryResponseDto(results, Relaxed: false);
    }

    private static CatalogProductDto MapToDto(Product p) => new(
        ProductId: p.ProductId,
        Name: p.Name,
        BrandId: p.BrandId,
        BrandName: p.Brand.Name,
        Category: p.Category,
        PriceEgp: p.PriceEgp,
        ImageUrl: p.ImageUrl,
        StyleTags: p.StyleTags ?? new List<string>(),
        SizesInStock: p.ProductSizes
            .Where(s => s.StockQty > 0)
            .Select(s => new SizeStockDto(s.Size, s.StockQty))
            .ToList(),
        CreatedAt: p.CreatedAt);
}