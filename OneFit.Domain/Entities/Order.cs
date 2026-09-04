using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class Order
{
    public string OrderId { get; set; } = null!;

    public string ShopperId { get; set; } = null!;

    public string CartId { get; set; } = null!;

    /// <summary>
    /// FR-23 — simulated only
    /// </summary>
    public string PaymentStatus { get; set; } = null!;

    /// <summary>
    /// must equal SUM(sub_orders.total_egp) — enforced in application code, not by the DB
    /// </summary>
    public decimal GrandTotalEgp { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Shopper Shopper { get; set; } = null!;

    public virtual ICollection<SubOrder> SubOrders { get; set; } = new List<SubOrder>();
}
