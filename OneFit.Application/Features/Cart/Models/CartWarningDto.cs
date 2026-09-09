using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Models
{
    public record CartWarningDto(
     string ProductId,
     string Code,
     decimal OldPriceEgp,
     decimal NewPriceEgp);
}
