namespace OneFit.Application.Features.Feed;

public sealed record FeedItemDto(
    string ProductId,
    string Name,
    string Brand,
    decimal PriceEgp,
    string? ImageUrl
);

public interface IFeedService
{
    Task<bool> ProductExistsAsync(string productId, CancellationToken ct = default);
    Task RecordAsync(string shopperId, string productId, string interactionType, CancellationToken ct = default);
    Task<List<FeedItemDto>> GetFeedAsync(string shopperId, int limit, CancellationToken ct = default);
}
