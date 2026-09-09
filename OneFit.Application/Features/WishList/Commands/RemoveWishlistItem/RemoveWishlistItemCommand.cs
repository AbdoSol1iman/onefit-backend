using MediatR;

namespace OneFit.Application.Features.WishList.Commands.RemoveWishlistItem
{
    /// <summary>
    /// Command to remove a wishlist item - requires ShopperId for authorization
    /// </summary>
    public record RemoveWishlistItemCommand(
        string ShopperId,
        string WishlistItemId) : IRequest<bool>;
}
