namespace OneFit.Domain.Entities;

public partial class UserInteraction
{
    public string InteractionId { get; set; } = null!;
    public string ShopperId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string InteractionType { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
    public virtual Shopper Shopper { get; set; } = null!;
}
