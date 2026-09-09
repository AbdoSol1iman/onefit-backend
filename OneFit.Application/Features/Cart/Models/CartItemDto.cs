using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Models
{
    public record CartItemDto(
    string ProductId,
    string Brand,
    decimal PriceEgp,
    int Qty);
}
