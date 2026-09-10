using Microsoft.EntityFrameworkCore;
using OneFit.Domain.Entities.Brands;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Brand> Brands { get; }

        DbSet<BrandDocument> BrandDocuments { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
