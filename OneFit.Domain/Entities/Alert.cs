using System;
using System.Collections.Generic;

namespace OneFit.Domain.Entities;

public partial class Alert
{
    public string AlertId { get; set; } = null!;

    public string ShopperId { get; set; } = null!;

    /// <summary>
    /// PRICE_DROP or LOW_STOCK
    /// </summary>
    public string AlertType { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    /// <summary>
    /// set only for LOW_STOCK alerts, per FR-26
    /// </summary>
    public string? Size { get; set; }

    public decimal? OldPriceEgp { get; set; }

    public decimal? NewPriceEgp { get; set; }

    public decimal? PercentSaved { get; set; }

    /// <summary>
    /// FR-28
    /// </summary>
    public bool IsRead { get; set; }

    public DateTime? TriggeredAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Shopper Shopper { get; set; } = null!;
}
