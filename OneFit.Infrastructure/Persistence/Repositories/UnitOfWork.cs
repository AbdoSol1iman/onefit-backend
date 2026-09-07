// OneFit.Infrastructure/Persistence/UnitOfWork.cs
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Persistence.Repositories;

namespace OneFit.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly OneFitDbContext _db; 
    private IProductRepository? _products;
    private IWishlistRepository? _wishlist;

    public UnitOfWork(OneFitDbContext db) => _db = db;

    public IProductRepository Products => _products ??= new ProductRepository(_db);
    public IWishlistRepository Wishlist => _wishlist ??= new WishlistRepository(_db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public void Dispose() => _db.Dispose();
}