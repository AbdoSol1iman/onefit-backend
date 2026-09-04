using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class Brand
{
    public string BrandId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<SubOrder> SubOrders { get; set; } = new List<SubOrder>();
}
