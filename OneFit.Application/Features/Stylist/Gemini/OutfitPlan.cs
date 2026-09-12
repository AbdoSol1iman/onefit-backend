using System.Text.Json;

namespace OneFit.Application.Features.Stylist.Gemini;

public sealed record OutfitSlot(
    string Slot,
    string Category,
    decimal? MaxPriceEgp = null,
    List<string>? StyleTags = null
);

public sealed record GeminiOutfitPlan(
    string? Occasion,
    decimal? Budget,
    List<OutfitSlot> ItemSlots,
    string Reasoning
);

public static class OutfitPlanValidator
{
    private static readonly HashSet<string> RootProps = ["occasion", "budget", "item_slots", "reasoning"];
    private static readonly HashSet<string> SlotProps = ["slot", "category", "max_price_egp", "style_tags"];

    public static bool TryParse(string? raw, out GeminiOutfitPlan? plan, out IReadOnlyList<string> errors)
    {
        plan = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            errors = ["response is empty"];
            return false;
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(raw);
        }
        catch (JsonException ex)
        {
            errors = [$"response is not valid JSON: {ex.Message}"];
            return false;
        }

        using (doc)
        {
            errors = Validate(doc.RootElement);
            if (errors.Count > 0)
                return false;
            plan = Map(doc.RootElement);
            return true;
        }
    }

    public static IReadOnlyList<string> Validate(JsonElement root)
    {
        var problems = new List<string>();
        if (root.ValueKind != JsonValueKind.Object)
            return ["root must be a JSON object"];

        foreach (var prop in root.EnumerateObject())
            if (!RootProps.Contains(prop.Name))
                problems.Add($"unexpected property '{prop.Name}'");

        if (!root.TryGetProperty("item_slots", out var slots) || slots.ValueKind != JsonValueKind.Array)
            problems.Add("item_slots is required and must be an array");
        else if (slots.GetArrayLength() is < 1 or > 6)
            problems.Add("item_slots must contain 1..6 entries");
        else
        {
            var i = 0;
            foreach (var slot in slots.EnumerateArray())
            {
                ValidateSlot(slot, i, problems);
                i++;
            }
        }

        if (!root.TryGetProperty("reasoning", out var reasoning) ||
            reasoning.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(reasoning.GetString()))
            problems.Add("reasoning is required and must be a non-empty string");

        if (root.TryGetProperty("budget", out var budget) &&
            budget.ValueKind is not (JsonValueKind.Number or JsonValueKind.Null) ||
            (root.TryGetProperty("budget", out var b2) && b2.ValueKind == JsonValueKind.Number && b2.GetDecimal() < 0))
            problems.Add("budget must be a number >= 0 when present");

        return problems;
    }

    private static void ValidateSlot(JsonElement slot, int index, List<string> problems)
    {
        if (slot.ValueKind != JsonValueKind.Object)
        {
            problems.Add($"item_slots[{index}] must be an object");
            return;
        }

        foreach (var prop in slot.EnumerateObject())
            if (!SlotProps.Contains(prop.Name))
                problems.Add($"item_slots[{index}] has unexpected property '{prop.Name}'");

        if (!slot.TryGetProperty("slot", out var name) ||
            name.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(name.GetString()))
            problems.Add($"item_slots[{index}].slot is required");

        if (!slot.TryGetProperty("category", out var category) ||
            category.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(category.GetString()))
            problems.Add($"item_slots[{index}].category is required");

        if (slot.TryGetProperty("max_price_egp", out var max) &&
            max.ValueKind is not (JsonValueKind.Number or JsonValueKind.Null) ||
            (slot.TryGetProperty("max_price_egp", out var m2) && m2.ValueKind == JsonValueKind.Number && m2.GetDecimal() < 0))
            problems.Add($"item_slots[{index}].max_price_egp must be a number >= 0 when present");

        if (slot.TryGetProperty("style_tags", out var tags) &&
            tags.ValueKind is not (JsonValueKind.Array or JsonValueKind.Null))
            problems.Add($"item_slots[{index}].style_tags must be an array when present");
    }

    private static GeminiOutfitPlan Map(JsonElement root)
    {
        string? occasion = root.TryGetProperty("occasion", out var o) && o.ValueKind == JsonValueKind.String
            ? o.GetString() : null;
        decimal? budget = root.TryGetProperty("budget", out var b) && b.ValueKind == JsonValueKind.Number
            ? b.GetDecimal() : null;
        var slots = root.GetProperty("item_slots").EnumerateArray().Select(s => new OutfitSlot(
            Slot: s.GetProperty("slot").GetString()!,
            Category: s.GetProperty("category").GetString()!,
            MaxPriceEgp: s.TryGetProperty("max_price_egp", out var m) && m.ValueKind == JsonValueKind.Number
                ? m.GetDecimal() : null,
            StyleTags: s.TryGetProperty("style_tags", out var t) && t.ValueKind == JsonValueKind.Array
                ? t.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!).ToList()
                : null)).ToList();
        return new GeminiOutfitPlan(occasion, budget, slots, root.GetProperty("reasoning").GetString()!);
    }
}
