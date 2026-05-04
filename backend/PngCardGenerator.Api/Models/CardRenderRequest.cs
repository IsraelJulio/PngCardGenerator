namespace PngCardGenerator.Api.Models;

public sealed class CardRenderRequest
{
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1400;
    public List<CardLayerDto> Layers { get; set; } = new();
}
