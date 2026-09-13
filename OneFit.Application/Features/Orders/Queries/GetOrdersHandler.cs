using MediatR;
using Microsoft.EntityFrameworkCore;
using OneFit.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Orders.Queries
{
    public class GetOrdersHandler
       : IRequestHandler<GetOrdersQuery, List<OrderDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetOrdersHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDto>> Handle(
            GetOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.ShopperId == request.ShopperId)
                .Include(o => o.SubOrders)
                    .ThenInclude(so => so.Brand)
                .Include(o => o.SubOrders)
                    .ThenInclude(so => so.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(order => new OrderDto
            {
                OrderId = order.OrderId,
                PaymentStatus = order.PaymentStatus,
                GrandTotalEgp = order.GrandTotalEgp,
                CreatedAt = order.CreatedAt,

                SubOrders = order.SubOrders
                    .Select(subOrder => new SubOrderDto
                    {
                        SubOrderId = subOrder.SubOrderId,
                        BrandId = subOrder.BrandId,
                        BrandName = subOrder.Brand.Name,
                        TotalEgp = subOrder.TotalEgp,

                        Items = subOrder.OrderItems
                            .Select(item => new OrderItemDto
                            {
                                OrderItemId = item.OrderItemId,
                                ProductId = item.ProductId,
                                ProductName = item.Product.Name,
                                Size = item.Size,
                                Qty = item.Qty,
                                PriceAtPurchaseEgp =
                                    item.PriceAtPurchaseEgp
                            })
                            .ToList()
                    })
                    .ToList()
            }).ToList();
        }
    }
}
