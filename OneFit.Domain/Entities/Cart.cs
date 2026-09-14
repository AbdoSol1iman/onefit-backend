using System;
using System.Collections.Generic;
using System.Linq;

namespace OneFit.Domain.Entities;

public partial class Cart
{
    public string CartId { get; set; } = null!;

    public string ShopperId { get; set; } = null!;

    /// <summary>
    /// active, checked_out
    /// </summary>
    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Shopper Shopper { get; set; } = null!;

    /// <summary>
    /// Adds an item to the cart. If the same product+size already exists,
    /// the quantity is <b>incremented</b> (not overwritten).
    /// </summary>
    public void AddItem(CartItem item)
    {
        if (item.Qty <= 0)
            throw new ArgumentOutOfRangeException(nameof(item), "Quantity must be greater than zero.");

        var existing = CartItems.FirstOrDefault(
            ci => ci.ProductId == item.ProductId && ci.Size == item.Size);

        if (existing is not null)
        {
            existing.Qty += item.Qty;
        }
        else
        {
            CartItems.Add(item);
        }
    }

    /// <summary>
    /// Removes a cart item by product and size. Returns true if removed, false if not found.
    /// </summary>
    public bool RemoveItem(string productId, string size)
    {
        var existing = CartItems.FirstOrDefault(
            ci => ci.ProductId == productId && ci.Size == size);

        if (existing is null)
            return false;

        CartItems.Remove(existing);
        return true;
    }

    /// <summary>
    /// Removes all items from the cart.
    /// </summary>
    public void Clear() => CartItems.Clear();
}
