using System.ComponentModel.DataAnnotations;

namespace OneFit.Api.Endpoints.WishList
{
    /// <summary>
    /// Request model for adding an item to the wishlist
    /// </summary>
    public record AddWishlistItemRequest(
        [Required(ErrorMessage = "ShopperId is required")]
        [StringLength(100, MinimumLength = 1)]
        string ShopperId,

        [Required(ErrorMessage = "ProductId is required")]
        [StringLength(100, MinimumLength = 1)]
        string ProductId,

        [Required(ErrorMessage = "Size is required")]
        [StringLength(50, MinimumLength = 1)]
        string Size);
}
