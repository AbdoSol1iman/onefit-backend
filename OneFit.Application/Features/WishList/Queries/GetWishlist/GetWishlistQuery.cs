using MediatR;
using OneFit.Application.Features.WishList.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.WishList.Queries.GetWishlist
{
    public record GetWishlistQuery(string ShopperId) : IRequest<IReadOnlyList<WishlistListItemDto>>;
}
