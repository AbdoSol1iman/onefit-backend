using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
