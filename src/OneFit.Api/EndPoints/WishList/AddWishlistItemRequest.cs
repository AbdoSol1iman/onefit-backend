namespace OneFit.Api.EndPoints.WishList
{
    public record AddWishlistItemRequest(
    string ShopperId,
    string ProductId,
    string Size);
}
