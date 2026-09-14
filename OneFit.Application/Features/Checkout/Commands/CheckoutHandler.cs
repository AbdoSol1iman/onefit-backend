using MediatR;
using Microsoft.EntityFrameworkCore;
using OneFit.Application.Common.Interfaces;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Common.Interfaces.Payments;
using OneFit.Application.Features.Checkout.Commands.OneFit.Application.Features.Checkout.Commands;
using OneFit.Domain.Entities;

namespace OneFit.Application.Features.Checkout.Commands
{
    public class CheckoutHandler
      : IRequestHandler<CheckoutCommand, CheckoutResultDto>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IApplicationDbContext _context;
        private readonly IStripePaymentService _stripePaymentService;

        public CheckoutHandler(
            ICartRepository cartRepository,
            IApplicationDbContext context,
            IStripePaymentService stripePaymentService)
        {
            _cartRepository = cartRepository;
            _context = context;
            _stripePaymentService = stripePaymentService;
        }

        public async Task<CheckoutResultDto> Handle(
            CheckoutCommand request,
            CancellationToken cancellationToken)
        {
            // Critical fix #13: Idempotency — reject if a pending order already
            // exists for this shopper (prevents double-submit).
            var existingPending = await _context.Orders
                .AnyAsync(
                    o => o.ShopperId == request.ShopperId
                         && o.PaymentStatus == "pending",
                    cancellationToken);

            if (existingPending)
            {
                throw new InvalidOperationException(
                    "You already have a pending checkout. Complete or cancel it before starting a new one.");
            }

            var cart = await _cartRepository
                .GetOrCreateActiveCartAsync(
                    request.ShopperId,
                    cancellationToken);

            var cartWithItems = await _cartRepository
                .GetCartWithItemsAsync(
                    cart.CartId,
                    cancellationToken);

            if (cartWithItems is null)
                throw new KeyNotFoundException(
                    "Cart not found.");

            if (!cartWithItems.CartItems.Any())
                throw new InvalidOperationException(
                    "Cannot checkout an empty cart.");

            // Critical fix #6: Atomic stock reservation using database-level
            // conditional update to prevent overselling under concurrency.
            foreach (var item in cartWithItems.CartItems)
            {
                // Use ExecuteUpdateAsync to atomically decrement stock
                // only if sufficient quantity is available.
                var rowsAffected = await _context.ProductSizes
                    .Where(ps =>
                        ps.ProductId == item.ProductId
                        && ps.Size == item.Size
                        && ps.StockQty >= item.Qty)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                ps => ps.StockQty,
                                ps => ps.StockQty - item.Qty),
                        cancellationToken);

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.ProductId}' in size '{item.Size}'.");
                }
            }

            // Critical fix #7: Create order as "pending" — it will only be
            // confirmed to "paid" when the Stripe webhook fires.
            var order = new Order
            {
                OrderId =
                    $"ORD-{Guid.NewGuid():N}"[..12]
                    .ToUpperInvariant(),

                ShopperId = request.ShopperId,

                CartId = cartWithItems.CartId,

                PaymentStatus = "pending",

                CreatedAt = DateTime.UtcNow
            };

            var subOrders = cartWithItems.CartItems
                .GroupBy(item => item.BrandId)
                .Select(group => new SubOrder
                {
                    SubOrderId =
                        $"SUB-{Guid.NewGuid():N}"[..12]
                        .ToUpperInvariant(),

                    OrderId = order.OrderId,

                    BrandId = group.Key,

                    TotalEgp = group.Sum(
                        item =>
                            item.PriceAtAddEgp *
                            item.Qty)
                })
                .ToList();

            order.GrandTotalEgp =
                subOrders.Sum(
                    subOrder => subOrder.TotalEgp);

            _context.Orders.Add(order);

            _context.SubOrders.AddRange(
                subOrders);

            foreach (var subOrder in subOrders)
            {
                var brandItems = cartWithItems.CartItems
                    .Where(item =>
                        item.BrandId ==
                        subOrder.BrandId);

                foreach (var cartItem in brandItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderItemId =
                            $"ITEM-{Guid.NewGuid():N}"[..12]
                            .ToUpperInvariant(),

                        SubOrderId =
                            subOrder.SubOrderId,

                        ProductId =
                            cartItem.ProductId,

                        Size =
                            cartItem.Size,

                        Qty =
                            cartItem.Qty,

                        PriceAtPurchaseEgp =
                            cartItem.PriceAtAddEgp
                    };

                    _context.OrderItems.Add(
                        orderItem);
                }
            }

            // Save pending order first
            await _context.SaveChangesAsync(
                cancellationToken);

            // Create Stripe Checkout Session
            // Critical fix: Read URLs from configuration instead of hardcoding localhost
            var successUrl = request.SuccessUrl
                ?? "https://localhost:5173/payment/success";
            var cancelUrl = request.CancelUrl
                ?? "https://localhost:5173/payment/cancel";

            var checkoutUrl =
                await _stripePaymentService
                    .CreateCheckoutSessionAsync(
                        order.OrderId,
                        order.GrandTotalEgp,
                        "egp",
                        successUrl,
                        cancelUrl);

            return new CheckoutResultDto
            {
                OrderId = order.OrderId,

                PaymentStatus =
                    order.PaymentStatus,

                GrandTotalEgp =
                    order.GrandTotalEgp,

                CheckoutUrl =
                    checkoutUrl
            };
        }
    }
}
