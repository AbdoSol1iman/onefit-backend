using OneFit.Application.Common.Interfaces.Payments;
using Stripe;

namespace OneFit.Api.Endpoints.Payments
{
    public static class StripeWebhookEndpoint
    {
        public static void MapStripeWebhookEndpoint(
            this IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/v1/payments/webhook",
                async (
                    HttpRequest request,
                    IStripeWebhookService webhookService) =>
                {
                    using var reader =
                        new StreamReader(
                            request.Body);

                    var json =
                        await reader.ReadToEndAsync();

                    var signature =
                        request.Headers[
                            "Stripe-Signature"]
                        .ToString();

                    if (string.IsNullOrEmpty(
                            signature))
                    {
                        return Results.BadRequest();
                    }

                    try
                    {
                        await webhookService
                            .HandleAsync(
                                json,
                                signature);

                        return Results.Ok();
                    }
                    catch (StripeException)
                    {
                        return Results.BadRequest();
                    }
                })
                .WithName("StripeWebhook")
                .WithTags("Payments");
        }
    }
}

