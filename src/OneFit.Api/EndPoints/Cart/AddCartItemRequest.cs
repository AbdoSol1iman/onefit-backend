using System.ComponentModel.DataAnnotations;

namespace OneFit.Api.EndPoints.Cart
{
    /// <summary>
    /// Request model for adding an item to the shopping cart
    /// </summary>
    public record AddCartItemRequest(
        [Required(ErrorMessage = "ShopperId is required")]
        [StringLength(100, MinimumLength = 1)]
        string ShopperId,

        [Required(ErrorMessage = "ProductId is required")]
        [StringLength(100, MinimumLength = 1)]
        string ProductId,

        [Required(ErrorMessage = "Size is required")]
        [StringLength(50, MinimumLength = 1)]
        string Size,

        [Range(1, int.MaxValue, ErrorMessage = "Qty must be greater than 0")]
        int Qty);
}
