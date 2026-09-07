namespace OneFit.Api.EndPoints.Cart
{
    public record AddCartItemRequest(
     string ShopperId,
     string ProductId,
     string Size,
     int Qty);
}
