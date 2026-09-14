using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        IWishlistRepository Wishlist { get; }
        ICartRepository Cart { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
