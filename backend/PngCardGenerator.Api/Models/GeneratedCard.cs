namespace PngCardGenerator.Api.Models;

public sealed class GeneratedCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? CardTemplateId { get; set; }
    public CardTemplate? Template { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int LayerCount { get; set; }
    public string RequestSnapshotJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
