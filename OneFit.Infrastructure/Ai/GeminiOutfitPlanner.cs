using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneFit.Application.Features.Stylist;
using OneFit.Application.Features.Stylist.Gemini;

namespace OneFit.Infrastructure.Ai;

public sealed class GeminiOutfitPlanner(
    HttpClient http,
    IOptions<GeminiOptions> options,
    IQuotaMonitor quota,
    ILogger<GeminiOutfitPlanner> logger) : IGeminiOutfitPlanner
{
    private const string Feature = "gemini-stylist";
    private readonly GeminiOptions _options = options.Value;

    public async Task<PlannerOutcome> PlanAsync(StylistIntent intent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            return new PlanSkipped();

        if (quota.IsThrottled(Feature))
        {
            logger.LogWarning("Gemini throttled locally for {Feature}, skipping API call", Feature);
            return new PlanRateLimited();
        }

        var first = await CallAsync(BuildPrompt(intent), corrective: null, ct);
        if (first is PlanRateLimited)
            return first;
        if (first is PlanSuccess)
            return first;

        var failed = (PlanFailed)first;
        var retry = await CallAsync(BuildPrompt(intent), string.Join("; ", failed.Errors), ct);
        if (retry is PlanSuccess or PlanRateLimited)
            return retry;

        var retryFailed = (PlanFailed)retry;
        logger.LogError("Gemini outfit plan failed validation twice. Errors: {Errors}. Raw: {Raw}",
            string.Join("; ", retryFailed.Errors), retryFailed.RawResponse);
        return retryFailed;
    }

    private static string BuildPrompt(StylistIntent intent) =>
        $"Plan a ready-to-buy outfit. Occasion: {intent.Occasion ?? "any"}, " +
        $"setting: {intent.Setting ?? "any"}, style: {intent.Style ?? "any"}, " +
        $"budget EGP: {(intent.BudgetEgp?.ToString() ?? "none")}. " +
        $"Return 1-3 item slots a shopper can buy now.";

    private async Task<PlannerOutcome> CallAsync(string task, string? corrective, CancellationToken ct)
    {
        var systemText = corrective is null
            ? "You are a fashion stylist. Respond with ONLY JSON matching the response schema. No prose."
            : $"You are a fashion stylist. Your previous response was invalid: {corrective}. " +
              "Respond with ONLY valid JSON matching the response schema. No prose.";

        var body = new JsonObject
        {
            ["systemInstruction"] = new JsonObject
            {
                ["parts"] = new JsonArray(new JsonObject { ["text"] = systemText }),
            },
            ["contents"] = new JsonArray(new JsonObject
            {
                ["role"] = "user",
                ["parts"] = new JsonArray(new JsonObject { ["text"] = task }),
            }),
            ["generationConfig"] = new JsonObject
            {
                ["temperature"] = 0.2,
                ["responseMimeType"] = "application/json",
                ["responseSchema"] = ResponseSchema(),
            },
        };

        using var req = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent");
        req.Headers.Add("x-goog-api-key", _options.ApiKey);
        req.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        HttpResponseMessage res;
        try
        {
            res = await http.SendAsync(req, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Gemini request failed");
            return new PlanFailed(null, ["transport error"]);
        }

        using (res)
        {
            var payload = await res.Content.ReadAsStringAsync(ct);
            if (res.StatusCode == HttpStatusCode.TooManyRequests)
            {
                quota.RecordQuotaHit(Feature);
                logger.LogWarning("Gemini quota exhausted (429) for {Feature}", Feature);
                return new PlanRateLimited();
            }

            if (!res.IsSuccessStatusCode)
            {
                logger.LogError("Gemini call failed with {Status}. Body: {Body}", (int)res.StatusCode, payload);
                return new PlanFailed(payload, [$"gemini error {(int)res.StatusCode}"]);
            }

            var raw = ExtractText(payload);
            return OutfitPlanValidator.TryParse(raw, out var plan, out var errors)
                ? new PlanSuccess(plan!)
                : new PlanFailed(raw, errors);
        }
    }

    private static string? ExtractText(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var parts = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts");
            var sb = new StringBuilder();
            foreach (var part in parts.EnumerateArray())
                if (part.TryGetProperty("text", out var text))
                    sb.Append(text.GetString());
            return sb.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static JsonObject ResponseSchema() => new()
    {
        ["type"] = "OBJECT",
        ["properties"] = new JsonObject
        {
            ["occasion"] = new JsonObject { ["type"] = "STRING" },
            ["budget"] = new JsonObject { ["type"] = "NUMBER" },
            ["item_slots"] = new JsonObject
            {
                ["type"] = "ARRAY",
                ["items"] = new JsonObject
                {
                    ["type"] = "OBJECT",
                    ["properties"] = new JsonObject
                    {
                        ["slot"] = new JsonObject { ["type"] = "STRING" },
                        ["category"] = new JsonObject { ["type"] = "STRING" },
                        ["max_price_egp"] = new JsonObject { ["type"] = "NUMBER" },
                        ["style_tags"] = new JsonObject
                        {
                            ["type"] = "ARRAY",
                            ["items"] = new JsonObject { ["type"] = "STRING" },
                        },
                    },
                    ["required"] = new JsonArray("slot", "category"),
                },
            },
            ["reasoning"] = new JsonObject { ["type"] = "STRING" },
        },
        ["required"] = new JsonArray("item_slots", "reasoning"),
    };
}
