using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Queries.GetCart
{
    public class GetCartQuery : IRequest<CartDto>
    {
        public string ShopperId { get; set; } = string.Empty;
    }
}
