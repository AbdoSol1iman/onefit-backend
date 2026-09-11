using MediatR;
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

            // Validate stock without deducting it
            foreach (var item in cartWithItems.CartItems)
            {
                if (item.ProductSize.StockQty < item.Qty)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.ProductId}' in size '{item.Size}'.");
                }
            }

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
            var checkoutUrl =
                await _stripePaymentService
                    .CreateCheckoutSessionAsync(
                        order.OrderId,
                        order.GrandTotalEgp,
                        "egp",
                        "https://localhost:5173/payment/success",
                        "https://localhost:5173/payment/cancel");

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
