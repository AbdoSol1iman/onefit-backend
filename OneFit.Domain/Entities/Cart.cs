using System;
using System.Collections.Generic;

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
}
