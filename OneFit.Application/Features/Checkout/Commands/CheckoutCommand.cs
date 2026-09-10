using MediatR;
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
