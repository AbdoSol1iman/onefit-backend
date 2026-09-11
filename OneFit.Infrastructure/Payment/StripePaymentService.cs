using Microsoft.Extensions.Options;
using OneFit.Application.Common.Interfaces.Payments;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Infrastructure.Payment
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly StripeSettings _settings;

        public StripePaymentService(
            IOptions<StripeSettings> settings)
        {
            _settings = settings.Value;

            StripeConfiguration.ApiKey =
                _settings.SecretKey;
        }

        public async Task<string> CreateCheckoutSessionAsync(
            string orderId,
            decimal amount,
            string currency,
            string successUrl,
            string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,

                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = orderId
                },

                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData =
                            new SessionLineItemPriceDataOptions
                            {
                                Currency = currency,

                                ProductData =
                                    new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name =
                                            $"OneFit Order {orderId}"
                                    },

                                UnitAmount =
                                    (long)(amount * 100)
                            },

                        Quantity = 1
                    }
                }
            };

            var service = new SessionService();

            var session =
                await service.CreateAsync(options);

            return session.Url;
        }
    }
}
