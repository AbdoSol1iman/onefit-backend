using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.WishList.Commands.RemoveWishlistItem
{
    public record RemoveWishlistItemCommand(string WishlistItemId) : IRequest<bool>;
}
