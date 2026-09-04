using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class Shopper
{
    public string ShopperId { get; set; } = null!;

    public string? Name { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
