using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities.Brands;

public partial class Brand
{
    public string BrandId { get; set; } = null!;

    public string Name { get; set; } = null!;

    // Identity user who owns this brand
    public string? ApplicationUserId { get; set; } = null!;

    // Brand verification status
    public BrandStatusEnum Status { get; set; }

    public bool IsLocal { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<BrandDocument> Documents { get; set; }
        = new List<BrandDocument>();

    public virtual ICollection<CartItem> CartItems { get; set; }
        = new List<CartItem>();

    public virtual ICollection<Product> Products { get; set; }
        = new List<Product>();

    public virtual ICollection<SubOrder> SubOrders { get; set; }
        = new List<SubOrder>();
}
