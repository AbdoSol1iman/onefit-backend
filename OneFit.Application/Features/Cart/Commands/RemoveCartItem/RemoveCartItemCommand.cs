using MediatR;
using OneFit.Application.Features.Cart.Models;

namespace OneFit.Application.Features.Cart.Commands.RemoveCartItem
{
    /// <summary>
    /// Command to remove a specific cart item by product ID and size
    /// </summary>
    public record RemoveCartItemCommand(
        string ShopperId,
        string ProductId,
        string Size) : IRequest<CartResponse>;
}
