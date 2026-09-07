using MediatR;
using OneFit.Application.Features.Catalog.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Catalog.Queries.QueryCatalog
{
    public record QueryCatalogQuery(
    string Category,
    decimal? MaxPriceEgp,
    IReadOnlyList<string>? StyleTags,
    bool InStockOnly,
    int Limit) : IRequest<CatalogQueryResponseDto>;
}
