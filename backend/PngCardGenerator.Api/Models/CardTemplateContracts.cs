namespace PngCardGenerator.Api.Models;

public sealed class CardTemplateUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1400;
    public List<CardLayerDto> Layers { get; set; } = new();
}

public class CardTemplateSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int LayerCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class CardTemplateDetailsDto : CardTemplateSummaryDto
{
    public List<CardLayerDto> Layers { get; set; } = new();
}
