using System.Collections.Concurrent;

namespace OneFit.Application.Features.Stylist;

public sealed class InMemoryStylistSessionStore : IStylistSessionStore
{
    private readonly ConcurrentDictionary<string, StylistSession> _sessions = new(StringComparer.OrdinalIgnoreCase);

    public StylistSession GetOrCreate(string shopperId) =>
        _sessions.GetOrAdd(shopperId, _ => new StylistSession(new StylistIntent()));

    public void Save(string shopperId, StylistSession session) =>
        _sessions[shopperId] = session;

    public void Reset(string shopperId) =>
        _sessions.TryRemove(shopperId, out _);
}
