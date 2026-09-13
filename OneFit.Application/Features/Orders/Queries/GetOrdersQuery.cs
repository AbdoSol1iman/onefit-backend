using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Orders.Queries
{
    public class GetOrdersQuery : IRequest<List<OrderDto>>
    {
        public string ShopperId { get; set; } = string.Empty;
    }
}
