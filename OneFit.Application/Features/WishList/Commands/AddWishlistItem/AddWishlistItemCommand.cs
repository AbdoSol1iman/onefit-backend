using MediatR;
using OneFit.Application.Features.WishList.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.WishList.Commands.AddWishlistItem
{
    public record AddWishlistItemCommand(
     string ShopperId,
     string ProductId,
     string Size) : IRequest<WishlistItemDto>;
}
