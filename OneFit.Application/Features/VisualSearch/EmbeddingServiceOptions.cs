namespace OneFit.Application.Features.VisualSearch;

public sealed class EmbeddingServiceOptions
{
    public const string SectionName = "EmbeddingService";
    public string BaseUrl { get; set; } = "https://onefit-ai-service.wittyocean-789a393e.germanywestcentral.azurecontainerapps.io";
    public int TimeoutSeconds { get; set; } = 60;
}
