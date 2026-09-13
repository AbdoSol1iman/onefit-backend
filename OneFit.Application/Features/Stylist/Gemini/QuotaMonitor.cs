using System.Collections.Concurrent;

namespace OneFit.Application.Features.Stylist.Gemini;

public interface IQuotaMonitor
{
    void RecordQuotaHit(string feature);
    int QuotaHits(string feature, TimeSpan? window = null);
    bool IsThrottled(string feature);
}

public sealed class InMemoryQuotaMonitor : IQuotaMonitor
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);
    private const int Threshold = 3;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTimeOffset>> _hits = new(StringComparer.OrdinalIgnoreCase);

    public void RecordQuotaHit(string feature) =>
        _hits.GetOrAdd(feature, _ => new ConcurrentQueue<DateTimeOffset>()).Enqueue(DateTimeOffset.UtcNow);

    public int QuotaHits(string feature, TimeSpan? window = null)
    {
        if (!_hits.TryGetValue(feature, out var queue))
            return 0;
        var cutoff = DateTimeOffset.UtcNow - (window ?? Window);
        while (queue.TryPeek(out var oldest) && oldest < cutoff)
            queue.TryDequeue(out _);
        return queue.Count;
    }

    public bool IsThrottled(string feature) => QuotaHits(feature) >= Threshold;
}
