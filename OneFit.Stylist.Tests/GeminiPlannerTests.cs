using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneFit.Application.Features.Products;
using OneFit.Application.Features.Stylist;
using OneFit.Application.Features.Stylist.Gemini;
using OneFit.Infrastructure.Ai;

namespace OneFit.Stylist.Tests;

internal sealed class NoopLogger<T> : ILogger<T>
{
    public static readonly NoopLogger<T> Instance = new();
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? ex, Func<TState, Exception?, string> formatter) { }
}

internal sealed class QueueHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();
    public readonly List<string> RequestBodies = new();

    public void Enqueue(HttpResponseMessage res) => _responses.Enqueue(res);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        RequestBodies.Add(await req.Content!.ReadAsStringAsync(ct));
        return _responses.Dequeue();
    }
}

public class GeminiPlannerTests
{
    private static readonly GeminiOptions Options = new() { ApiKey = "test-key", Model = "gemini-2.0-flash" };

    private static HttpResponseMessage GeminiOk(string planJson) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    candidates = new[] { new { content = new { parts = new[] { new { text = planJson } } } } },
                }), Encoding.UTF8, "application/json"),
        };

    private static HttpResponseMessage RateLimited() =>
        new(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(
                """{"error":{"code":429,"message":"Quota exceeded","status":"RESOURCE_EXHAUSTED"}}""",
                Encoding.UTF8, "application/json"),
        };

    private const string ValidPlan =
        """{"occasion":"wedding","budget":2500,"item_slots":[{"slot":"top","category":"shirt","max_price_egp":1500,"style_tags":["casual"]}],"reasoning":"Beach wedding, keep it light"}""";

    private static (GeminiOutfitPlanner planner, QueueHandler handler, InMemoryQuotaMonitor quota) Build(
        string? apiKey = "test-key")
    {
        var handler = new QueueHandler();
        var quota = new InMemoryQuotaMonitor();
        var planner = new GeminiOutfitPlanner(
            new HttpClient(handler),
            Microsoft.Extensions.Options.Options.Create(new GeminiOptions { ApiKey = apiKey ?? string.Empty }),
            quota,
            NoopLogger<GeminiOutfitPlanner>.Instance);
        return (planner, handler, quota);
    }

    [Fact]
    public async Task ValidResponse_ReturnsPlan_WithoutProse()
    {
        var (planner, handler, _) = Build();
        handler.Enqueue(GeminiOk(ValidPlan));

        var res = await planner.PlanAsync(new StylistIntent("wedding", "beach", "casual", 2500));

        var ok = Assert.IsType<PlanSuccess>(res);
        Assert.Equal("wedding", ok.Plan.Occasion);
        Assert.Single(ok.Plan.ItemSlots);
        Assert.Equal("shirt", ok.Plan.ItemSlots[0].Category);
        Assert.Single(handler.RequestBodies);
        Assert.Contains("responseSchema", handler.RequestBodies[0]);
    }

    [Fact]
    public async Task MalformedThenValid_RetriesOnce_WithCorrectiveInstruction()
    {
        var (planner, handler, _) = Build();
        handler.Enqueue(GeminiOk("Sure! Here is your outfit: a nice shirt..."));
        handler.Enqueue(GeminiOk(ValidPlan));

        var res = await planner.PlanAsync(new StylistIntent("wedding", "beach", "casual", 2500));

        Assert.IsType<PlanSuccess>(res);
        Assert.Equal(2, handler.RequestBodies.Count);
        Assert.Contains("invalid", handler.RequestBodies[1].ToLowerInvariant());
    }

    [Fact]
    public async Task MalformedTwice_ReturnsFailed_AfterExactlyTwoCalls()
    {
        var (planner, handler, _) = Build();
        handler.Enqueue(GeminiOk("not json at all"));
        handler.Enqueue(GeminiOk("""{"wrong":"shape"}"""));

        var res = await planner.PlanAsync(new StylistIntent("wedding", "beach", "casual", 2500));

        var failed = Assert.IsType<PlanFailed>(res);
        Assert.NotNull(failed.RawResponse);
        Assert.NotEmpty(failed.Errors);
        Assert.Equal(2, handler.RequestBodies.Count);
    }

    [Fact]
    public async Task DoubleFailure_Orchestrator_ShowsFallback_WithoutCatalog()
    {
        var (planner, handler, _) = Build();
        handler.Enqueue(GeminiOk("garbage"));
        handler.Enqueue(GeminiOk("""{"wrong":"shape"}"""));
        var fake = new FakeProductQueryService();
        var sut = new StylistOrchestrator(new InMemoryStylistSessionStore(), fake, planner);

        var res = await sut.HandleAsync("g1", "عايز طقم كاجوال لفرح على البحر بميزانية 2500 جنيه");

        Assert.Equal("llm_fallback", res.Status);
        Assert.Contains("rephrasing", res.Reply);
        Assert.Empty(res.Outfits);
        Assert.False(res.CatalogCalled);
        Assert.Equal(0, fake.Calls);
    }

    [Fact]
    public async Task RateLimit_ReturnsBusy_NoRetry_RecordsQuota()
    {
        var (planner, handler, quota) = Build();
        handler.Enqueue(RateLimited());

        var res = await planner.PlanAsync(new StylistIntent("wedding", "beach", "casual", 2500));

        Assert.IsType<PlanRateLimited>(res);
        Assert.Single(handler.RequestBodies);
        Assert.Equal(1, quota.QuotaHits("gemini-stylist"));
    }

    [Fact]
    public async Task RateLimit_Orchestrator_ShowsBusyMessage()
    {
        var (planner, handler, _) = Build();
        handler.Enqueue(RateLimited());
        var fake = new FakeProductQueryService();
        var sut = new StylistOrchestrator(new InMemoryStylistSessionStore(), fake, planner);

        var res = await sut.HandleAsync("g2", "عايز طقم كاجوال لفرح على البحر بميزانية 2500 جنيه");

        Assert.Equal("stylist_busy", res.Status);
        Assert.Contains("busy", res.Reply);
        Assert.False(res.CatalogCalled);
        Assert.Equal(0, fake.Calls);
    }

    [Fact]
    public async Task RepeatedQuotaHits_ThrottleLocally_WithoutNewHttpCall()
    {
        var (planner, handler, quota) = Build();
        quota.RecordQuotaHit("gemini-stylist");
        quota.RecordQuotaHit("gemini-stylist");
        quota.RecordQuotaHit("gemini-stylist");

        var res = await planner.PlanAsync(new StylistIntent("wedding", "beach", "casual", 2500));

        Assert.IsType<PlanRateLimited>(res);
        Assert.Empty(handler.RequestBodies);
    }

    [Fact]
    public async Task NoApiKey_SkipsGemini_FallsBackToDirectAssembly()
    {
        var (planner, handler, _) = Build(apiKey: null);
        var fake = new FakeProductQueryService();
        var sut = new StylistOrchestrator(new InMemoryStylistSessionStore(), fake, planner);

        var res = await sut.HandleAsync("g3", "عايز طقم كاجوال لفرح على البحر بميزانية 2500 جنيه");

        Assert.Equal("ready", res.Status);
        Assert.True(res.CatalogCalled);
        Assert.Empty(handler.RequestBodies);
    }

    [Fact]
    public void Validator_RejectsExtraProperties()
    {
        using var doc = JsonDocument.Parse(
            """{"occasion":"wedding","item_slots":[{"slot":"top","category":"shirt"}],"reasoning":"ok","free_prose":"hello"}""");
        var errors = OutfitPlanValidator.Validate(doc.RootElement);
        Assert.Contains(errors, e => e.Contains("free_prose"));
    }

    [Fact]
    public void QuotaMonitor_WindowExpires()
    {
        var quota = new InMemoryQuotaMonitor();
        quota.RecordQuotaHit("x");
        Assert.Equal(1, quota.QuotaHits("x"));
        Assert.Equal(0, quota.QuotaHits("x", TimeSpan.Zero));
    }
}
