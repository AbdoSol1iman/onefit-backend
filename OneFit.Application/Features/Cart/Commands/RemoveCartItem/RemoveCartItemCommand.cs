using MediatR;
using OneFit.Application.Features.Cart.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Commands.RemoveCartItem
{
    public record RemoveCartItemCommand(string ShopperId, string ProductId) : IRequest<CartResponse>;
}
