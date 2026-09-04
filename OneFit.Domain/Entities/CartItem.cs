using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class CartItem
{
    public string CartItemId { get; set; } = null!;

    public string CartId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    /// <summary>
    /// denormalized for FR-18 per-line-item brand display without a join
    /// </summary>
    public string BrandId { get; set; } = null!;

    public string Size { get; set; } = null!;

    public int Qty { get; set; }

    /// <summary>
    /// snapshot for FR-09/NFR-09 re-validation at checkout
    /// </summary>
    public decimal PriceAtAddEgp { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual Cart Cart { get; set; } = null!;

    public virtual ProductSize ProductSize { get; set; } = null!;
}
