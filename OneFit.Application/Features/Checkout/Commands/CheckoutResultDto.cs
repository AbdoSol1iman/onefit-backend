using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Checkout.Commands
{
    namespace OneFit.Application.Features.Checkout.Commands
    {
        public class CheckoutResultDto
        {
            public string OrderId { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
            public decimal GrandTotalEgp { get; set; }
            public string CheckoutUrl { get; set; } = string.Empty;
        }
    }
}
