using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OneFit.Api.Endpoints.Cart
{
    /// <summary>
    /// Request model for adding an item to the shopping cart.
    /// ShopperId is no longer accepted from the client — it is
    /// extracted from the JWT token in the endpoint handler.
    /// </summary>
    public record AddCartItemRequest(
        [property: JsonPropertyName("product_id")]
        [Required(ErrorMessage = "ProductId is required")]
        [StringLength(100, MinimumLength = 1)]
        string ProductId,

        [property: JsonPropertyName("size")]
        [Required(ErrorMessage = "Size is required")]
        [StringLength(50, MinimumLength = 1)]
        string Size,

        [property: JsonPropertyName("qty")]
        [Range(1, int.MaxValue, ErrorMessage = "Qty must be greater than 0")]
        int Qty);
}
