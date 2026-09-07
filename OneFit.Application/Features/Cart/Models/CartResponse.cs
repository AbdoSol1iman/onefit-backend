using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Models
{
    public record CartResponse(
    string CartId,
    IReadOnlyList<CartItemDto> Items,
    decimal TotalEgp,
    IReadOnlyList<CartWarningDto>? Warnings = null);
}
