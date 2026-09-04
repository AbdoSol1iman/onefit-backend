using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class OrderItem
{
    public string OrderItemId { get; set; } = null!;

    public string SubOrderId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string Size { get; set; } = null!;

    public int Qty { get; set; }

    /// <summary>
    /// final locked-in price at checkout — independent of later product.price_egp changes
    /// </summary>
    public decimal PriceAtPurchaseEgp { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual SubOrder SubOrder { get; set; } = null!;
}
