using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.Catalog
{
    public record CatalogFilter(
     string Category,
     decimal? MaxPriceEgp,
     IReadOnlyList<string>? StyleTags,
     bool InStockOnly,
     int Limit);
}
