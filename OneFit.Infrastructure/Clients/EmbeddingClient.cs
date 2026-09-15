using System.Net.Http.Json;
using System.Text.Json;
using OneFit.Application.Features.VisualSearch;

namespace OneFit.Infrastructure.Clients;

public sealed class EmbeddingClient : IEmbeddingClient
{
    private readonly HttpClient _http;

    public EmbeddingClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<float[]?> GenerateEmbeddingAsync(string imageBase64, CancellationToken ct = default)
    {
        var payload = new { image_base64 = imageBase64 };
        using var response = await _http.PostAsJsonAsync("api/v1/internal/generate-embedding", payload, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("embedding", out var embeddingElement))
            return null;

        var embedding = new float[embeddingElement.GetArrayLength()];
        int i = 0;
        foreach (var val in embeddingElement.EnumerateArray())
        {
            embedding[i++] = val.GetSingle();
        }

        return embedding;
    }
}
