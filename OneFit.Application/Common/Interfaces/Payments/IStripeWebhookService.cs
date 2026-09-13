using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.Payments
{
    public interface IStripeWebhookService
    {
        Task HandleAsync(
            string json,
            string signature);
    }
}
