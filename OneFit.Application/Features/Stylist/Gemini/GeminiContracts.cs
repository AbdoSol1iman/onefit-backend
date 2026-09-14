namespace OneFit.Application.Features.Stylist.Gemini;

public sealed class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.0-flash";
    public int MaxSlots { get; set; } = 3;
}

public abstract record PlannerOutcome;
public sealed record PlanSuccess(GeminiOutfitPlan Plan) : PlannerOutcome;
public sealed record PlanFailed(string? RawResponse, IReadOnlyList<string> Errors) : PlannerOutcome;
public sealed record PlanRateLimited : PlannerOutcome;
public sealed record PlanSkipped : PlannerOutcome;

public interface IGeminiOutfitPlanner
{
    Task<PlannerOutcome> PlanAsync(StylistIntent intent, CancellationToken ct = default);
}
