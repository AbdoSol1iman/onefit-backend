using MediatR;
using OneFit.Application.Features.Checkout.Commands.OneFit.Application.Features.Checkout.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Checkout.Commands
{
    public class CheckoutCommand : IRequest<CheckoutResultDto>
    {
        public string ShopperId { get; set; } = string.Empty;
    }
}
