using OneFit.Application.Common.Interfaces.Catalog;
using OneFit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.IRepositories
{
    public interface IProductRepository
    {
        Task<IReadOnlyList<Product>> QueryAsync(CatalogFilter filter, CancellationToken ct = default);
    }
}
