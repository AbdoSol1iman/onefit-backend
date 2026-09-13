using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Queries.GetCart
{
    public class CartDto
    {
        public string CartId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public List<CartItemDto> Items { get; set; } = [];
        public decimal Total { get; set; }
    }
}
