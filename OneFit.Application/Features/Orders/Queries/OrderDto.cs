using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Orders.Queries
{
    public class OrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal GrandTotalEgp { get; set; }
        public DateTime? CreatedAt { get; set; }

        public List<SubOrderDto> SubOrders { get; set; } = [];
    }

    public class SubOrderDto
    {
        public string SubOrderId { get; set; } = string.Empty;
        public string BrandId { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal TotalEgp { get; set; }

        public List<OrderItemDto> Items { get; set; } = [];
    }

    public class OrderItemDto
    {
        public string OrderItemId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal PriceAtPurchaseEgp { get; set; }
    }
}
