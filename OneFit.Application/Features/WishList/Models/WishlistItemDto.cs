using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.WishList.Models
{
    public record WishlistItemDto(
    string WishlistItemId,
    string ProductId,
    string Size,
    decimal PriceAtSaveEgp,
    DateTime? AddedAt);
}
