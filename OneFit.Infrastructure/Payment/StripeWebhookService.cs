using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OneFit.Application.Common.Interfaces;
using OneFit.Application.Common.Interfaces.Payments;
using Stripe;
using Stripe.Checkout;

namespace OneFit.Infrastructure.Payment
{
    public class StripeWebhookService
        : IStripeWebhookService
    {
        private readonly StripeSettings _settings;
        private readonly IApplicationDbContext _context;

        public StripeWebhookService(
            IOptions<StripeSettings> settings,
            IApplicationDbContext context)
        {
            _settings = settings.Value;
            _context = context;
        }

        public async Task HandleAsync(
            string json,
            string signature)
        {
            var stripeEvent =
                EventUtility.ConstructEvent(
                    json,
                    signature,
                    _settings.WebhookSecret);

            if (stripeEvent.Type !=
                EventTypes.CheckoutSessionCompleted)
            {
                return;
            }

            var session =
                stripeEvent.Data.Object as Session;

            if (session is null)
                return;

            if (!string.Equals(
                    session.PaymentStatus,
                    "paid",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!session.Metadata.TryGetValue(
                    "order_id",
                    out var orderId))
            {
                return;
            }

            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(
                        x => x.OrderId == orderId);

            if (order is null)
                return;

            // Prevent duplicate processing
            if (string.Equals(
                    order.PaymentStatus,
                    "paid",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var subOrders =
                await _context.SubOrders
                    .Where(x =>
                        x.OrderId == orderId)
                    .ToListAsync();

            var subOrderIds =
                subOrders
                    .Select(x => x.SubOrderId)
                    .ToList();

            var orderItems =
                await _context.OrderItems
                    .Where(x =>
                        subOrderIds.Contains(
                            x.SubOrderId))
                    .ToListAsync();

            foreach (var orderItem in orderItems)
            {
                var productSize =
                    await _context.ProductSizes
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                orderItem.ProductId
                            &&
                            x.Size ==
                                orderItem.Size);

                if (productSize is null)
                    continue;

                if (productSize.StockQty <
                    orderItem.Qty)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{orderItem.ProductId}' in size '{orderItem.Size}'.");
                }

                productSize.StockQty -=
                    orderItem.Qty;
            }

            // Payment succeeded
            order.PaymentStatus = "paid";

            // Close and clear the cart
            var cart =
                await _context.Carts
                    .Include(x => x.CartItems)
                    .FirstOrDefaultAsync(
                        x => x.CartId == order.CartId);

            if (cart is not null)
            {
                cart.Status = "checked_out";

                _context.CartItems.RemoveRange(
                    cart.CartItems);
            }

            await _context.SaveChangesAsync();
        }
    }

}
