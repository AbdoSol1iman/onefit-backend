using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class ProductSize
{
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// S, M, L, XL, etc.
    /// </summary>
    public string Size { get; set; } = null!;

    public int StockQty { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
