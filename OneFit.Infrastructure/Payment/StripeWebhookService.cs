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

            // Critical fix #8: Batch-fetch all required ProductSizes in one query
            // instead of N+1, and handle insufficient stock gracefully.
            var productSizeKeys = orderItems
                .Select(oi => new { oi.ProductId, oi.Size })
                .ToList();

            var productSizes =
                await _context.ProductSizes
                    .Where(ps => orderItems
                        .Select(oi => oi.ProductId)
                        .Contains(ps.ProductId))
                    .ToListAsync();

            var hasStockIssue = false;

            foreach (var orderItem in orderItems)
            {
                var productSize = productSizes
                    .FirstOrDefault(ps =>
                        ps.ProductId == orderItem.ProductId
                        && ps.Size == orderItem.Size);

                if (productSize is null)
                    continue;

                if (productSize.StockQty <
                    orderItem.Qty)
                {
                    // Critical fix #8: Don't throw — mark for manual review.
                    // Throwing causes Stripe to retry indefinitely while the
                    // customer is charged but never fulfilled.
                    hasStockIssue = true;
                    continue;
                }

                productSize.StockQty -=
                    orderItem.Qty;
            }

            // Payment succeeded — mark accordingly
            order.PaymentStatus = hasStockIssue
                ? "paid_requires_attention"
                : "paid";

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

            // Always return 200 to Stripe (via successful completion)
            await _context.SaveChangesAsync();
        }
    }

}
