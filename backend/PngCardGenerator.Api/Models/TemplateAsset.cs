namespace PngCardGenerator.Api.Models;

public sealed class TemplateAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MimeType { get; set; } = "image/png";
    public string Base64Data { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<CardTemplateLayer> Layers { get; set; } = new();
}
