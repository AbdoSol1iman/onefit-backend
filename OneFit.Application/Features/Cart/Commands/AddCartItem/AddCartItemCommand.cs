using MediatR;
using OneFit.Application.Features.Cart.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Commands.AddCartItem
{
    public record AddCartItemCommand(
    string ShopperId,
    string ProductId,
    string Size,
    int Qty) : IRequest<CartResponse>;
}
