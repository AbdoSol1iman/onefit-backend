using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.Payments
{
    public interface IStripePaymentService
    {
        Task<string> CreateCheckoutSessionAsync(
            string orderId,
            decimal amount,
            string currency,
            string successUrl,
            string cancelUrl);
    }
}
