using OneFit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.IRepositories
{
    public interface ICartRepository
    {

        Task<Cart> GetOrCreateActiveCartAsync(string shopperId, CancellationToken ct = default);
        Task<ProductSize?> FindProductSizeAsync(string productId, string size, CancellationToken ct = default);

        Task<CartItem?> FindLineItemAsync(string cartId, string productId, string size, CancellationToken ct = default);

        Task AddLineItemAsync(CartItem item, CancellationToken ct = default);

        void RemoveLineItem(CartItem item);


        Task<Cart?> GetCartWithItemsAsync(string cartId, CancellationToken ct = default);
    }
}
