using OneFit.Domain.Entities.Brands;
using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class Product
{
    public string ProductId { get; set; } = null!;

    public string BrandId { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary>
    /// shirt, pants, shoes, accessory, etc.
    /// </summary>
    public string Category { get; set; } = null!;

    public decimal PriceEgp { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>
    /// 512-D L2-normalized image embedding for visual similarity search (pgvector).
    /// </summary>
    public float[]? ImageEmbedding { get; set; }

    /// <summary>
    /// e.g. linen, casual, chino — used by Catalog-Query Tool
    /// </summary>
    public List<string>? StyleTags { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
}
