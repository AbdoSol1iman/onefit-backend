using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.WishList.Models
{
    public record WishlistListItemDto(
    string WishlistItemId,
    string ProductId,
    string BrandName,
    string Size,
    decimal PriceEgp,
    decimal PriceAtSaveEgp,
    bool InStock);
}
