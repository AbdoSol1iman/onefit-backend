using System.ComponentModel.DataAnnotations;

namespace OneFit.Api.Endpoints.WishList
{
    /// <summary>
    /// Request model for adding an item to the wishlist.
    /// ShopperId is no longer accepted from the client — it is
    /// extracted from the JWT token in the endpoint handler.
    /// </summary>
    public record AddWishlistItemRequest(
        [Required(ErrorMessage = "ProductId is required")]
        [StringLength(100, MinimumLength = 1)]
        string ProductId,

        [Required(ErrorMessage = "Size is required")]
        [StringLength(50, MinimumLength = 1)]
        string Size);
}
