using System.Text.RegularExpressions;

namespace OneFit.Application.Features.Stylist;

internal static class StylistIntentExtractor
{
    private static readonly (string Value, string[] Keys)[] Occasions =
    [
        ("wedding", ["فرح", "زفاف", "wedding"]),
        ("work", ["شغل", "عمل", "interview", "work", "office"]),
        ("party", ["سهرة", "حفلة", "party"]),
        ("beach_day", ["مصيف", "beach day"]),
    ];

    private static readonly (string Value, string[] Keys)[] Settings =
    [
        ("beach", ["بحر", "شاطئ", "beach", "sea"]),
        ("city", ["مدينة", "city", "downtown"]),
        ("indoor", ["داخلي", "indoor", "hall", "قاعة"]),
    ];

    private static readonly (string Value, string[] Keys)[] Styles =
    [
        ("casual", ["كاجوال", "casual"]),
        ("formal", ["رسمي", "فورمال", "formal", "classic", "كلاسيك"]),
        ("sport", ["رياضي", "sport", "sporty"]),
        ("linen", ["كتان", "linen"]),
    ];

    private static readonly string[] FashionSignals =
    [
        "طقم", "outfit", "لبس", "ستايل", "style", "occasion", "فرح", "زفاف",
        "بحر", "شاطئ", "beach", "كاجوال", "casual", "رسمي", "formal",
        "ميزانية", "budget", "جنيه", "egp", "موضة", "fashion", "look", "لوك",
        "شغل", "سهرة", "حفلة", "قاعة", "مصيف", "wedding", "party", "work",
    ];

    private static readonly string[] SkipSignals =
    [
        "skip", "تخطي", "بدون ميزانية", "من غير ميزانية", "no budget", "skip budget",
    ];

    internal static bool IsSkip(string message)
    {
        var lower = message.ToLowerInvariant();
        return SkipSignals.Any(s => lower.Contains(s));
    }

    internal static StylistIntent Extract(string message)
    {
        var lower = message.ToLowerInvariant();
        return new StylistIntent(
            Occasion: MatchFirst(lower, Occasions),
            Setting: MatchFirst(lower, Settings),
            Style: MatchFirst(lower, Styles),
            BudgetEgp: ExtractBudget(message));
    }

    internal static bool HasAnyField(StylistIntent intent) =>
        intent.Occasion is not null || intent.Setting is not null ||
        intent.Style is not null || intent.BudgetEgp.HasValue;

    internal static bool LooksFashionRelated(string message)
    {
        var lower = message.ToLowerInvariant();
        return FashionSignals.Any(s => lower.Contains(s)) || ExtractBudget(message).HasValue;
    }

    internal static StylistIntent Merge(StylistIntent stored, StylistIntent fresh) =>
        new(
            Occasion: fresh.Occasion ?? stored.Occasion,
            Setting: fresh.Setting ?? stored.Setting,
            Style: fresh.Style ?? stored.Style,
            BudgetEgp: fresh.BudgetEgp ?? stored.BudgetEgp,
            BudgetSkipped: stored.BudgetSkipped);

    internal static List<string>? ToStyleTags(string? style) =>
        style is null ? null : [style.ToLowerInvariant()];

    private static string? MatchFirst(string lower, (string Value, string[] Keys)[] map)
    {
        foreach (var (value, keys) in map)
            foreach (var key in keys)
                if (lower.Contains(key.ToLowerInvariant()))
                    return value;
        return null;
    }

    private static decimal? ExtractBudget(string message)
    {
        var normalized = NormalizeDigits(message);
        var match = Regex.Match(normalized, @"(\d[\d,\.]*)");
        if (!match.Success)
            return null;
        var raw = match.Groups[1].Value.Replace(",", string.Empty);
        return decimal.TryParse(raw, out var budget) && budget > 0 ? budget : null;
    }

    private static string NormalizeDigits(string value)
    {
        var arabic = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
        var sb = new System.Text.StringBuilder(value.Length);
        foreach (var ch in value)
        {
            var idx = Array.IndexOf(arabic, ch);
            sb.Append(idx >= 0 ? (char)('0' + idx) : ch);
        }
        return sb.ToString();
    }
}
