using OneFit.Domain.Entities.Brands;
using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class SubOrder
{
    public string SubOrderId { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public string BrandId { get; set; } = null!;

    /// <summary>
    /// FR-21 — one sub-order per brand; must equal SUM(order_items.price_at_purchase_egp * qty)
    /// </summary>
    public decimal TotalEgp { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
