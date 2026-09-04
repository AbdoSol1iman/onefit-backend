using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class WishlistItem
{
    public string WishlistItemId { get; set; } = null!;

    public string ShopperId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    /// <summary>
    /// needed for FR-26 per-size stock alerts
    /// </summary>
    public string Size { get; set; } = null!;

    /// <summary>
    /// snapshot used to detect PRICE_DROP per FR-25
    /// </summary>
    public decimal PriceAtSaveEgp { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual ProductSize ProductSize { get; set; } = null!;

    public virtual Shopper Shopper { get; set; } = null!;
}
