using OneFit.Application.Features.Products;
using OneFit.Application.Features.Stylist;

namespace OneFit.Stylist.Tests;

internal sealed class FakeProductQueryService : IProductQueryService
{
    public int Calls { get; private set; }
    public ProductListQuery? LastQuery { get; private set; }

    public Task<PagedResult<ProductSummaryDto>> ListAsync(ProductListQuery query, CancellationToken ct = default)
    {
        Calls++;
        LastQuery = query;
        var items = new List<ProductSummaryDto>
        {
            new("p1", "Brand", "Shirt", 500, ["M"], null),
        };
        return Task.FromResult(new PagedResult<ProductSummaryDto>(items, 1, query.Limit ?? query.PageSize, 1));
    }

    public Task<ProductDetailDto?> GetByIdAsync(string productId, CancellationToken ct = default) =>
        Task.FromResult<ProductDetailDto?>(null);
}

public class StylistOrchestratorTests
{
    private static StylistOrchestrator Build(out FakeProductQueryService fake) =>
        new(new InMemoryStylistSessionStore(), fake = new FakeProductQueryService());

    [Fact]
    public async Task WellFormedRequest_ExtractsIntent_AndCallsCatalog()
    {
        var sut = Build(out var fake);

        var res = await sut.HandleAsync("s1", "عايز طقم كاجوال لفرح على البحر بميزانية 2500 جنيه");

        Assert.Equal("ready", res.Status);
        Assert.True(res.CatalogCalled);
        Assert.Equal(1, fake.Calls);
        Assert.Equal("wedding", res.Intent.Occasion);
        Assert.Equal("beach", res.Intent.Setting);
        Assert.Equal("casual", res.Intent.Style);
        Assert.Equal(2500, res.Intent.BudgetEgp);
        Assert.Equal(2500, fake.LastQuery!.MaxPriceEgp);
        Assert.Equal(3, fake.LastQuery.Limit);
        Assert.NotEmpty(res.Outfits);
    }

    [Fact]
    public async Task MissingBudget_AsksFollowUp_AndWithholdsCatalog()
    {
        var sut = Build(out var fake);

        var res = await sut.HandleAsync("s2", "عايز طقم كاجوال لفرح على البحر");

        Assert.Equal("need_budget", res.Status);
        Assert.False(res.CatalogCalled);
        Assert.Equal(0, fake.Calls);
        Assert.Contains("budget", res.Reply.ToLowerInvariant());
    }

    [Fact]
    public async Task MissingBudgetThenProvided_AssemblesOutfit()
    {
        var store = new InMemoryStylistSessionStore();
        var fake = new FakeProductQueryService();
        var sut = new StylistOrchestrator(store, fake);

        await sut.HandleAsync("s3", "عايز طقم كاجوال لفرح على البحر");
        var res = await sut.HandleAsync("s3", "2500 جنيه");

        Assert.Equal("ready", res.Status);
        Assert.True(res.CatalogCalled);
        Assert.Equal("casual", res.Intent.Style);
        Assert.Equal(2500, res.Intent.BudgetEgp);
    }

    [Fact]
    public async Task MissingBudgetThenSkipped_AssemblesWithoutBudget()
    {
        var store = new InMemoryStylistSessionStore();
        var fake = new FakeProductQueryService();
        var sut = new StylistOrchestrator(store, fake);

        await sut.HandleAsync("s4", "عايز طقم كاجوال لفرح على البحر");
        var res = await sut.HandleAsync("s4", "skip");

        Assert.Equal("ready", res.Status);
        Assert.True(res.CatalogCalled);
        Assert.Null(fake.LastQuery!.MaxPriceEgp);
    }

    [Fact]
    public async Task OffTopic_Redirects_AndSkipsCatalog()
    {
        var sut = Build(out var fake);

        var res = await sut.HandleAsync("s5", "what's the weather today");

        Assert.Equal("off_topic", res.Status);
        Assert.False(res.CatalogCalled);
        Assert.Equal(0, fake.Calls);
        Assert.Contains("outfit", res.Reply.ToLowerInvariant());
    }

    [Fact]
    public async Task Sessions_ArePerShopper()
    {
        var store = new InMemoryStylistSessionStore();
        var fake = new FakeProductQueryService();
        var sut = new StylistOrchestrator(store, fake);

        await sut.HandleAsync("a", "عايز طقم كاجوال لفرح على البحر");
        var res = await sut.HandleAsync("b", "عايز طقم كاجوال لفرح على البحر بميزانية 2500 جنيه");

        Assert.Equal("ready", res.Status);
        Assert.Equal(1, fake.Calls);
    }
}
