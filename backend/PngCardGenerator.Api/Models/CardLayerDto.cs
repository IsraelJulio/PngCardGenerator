namespace PngCardGenerator.Api.Models;

public sealed class CardLayerDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public CardLayerType Type { get; set; }

    public string? Name { get; set; }

    public string? ImageBase64 { get; set; }
    public string? Text { get; set; }

    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public float Rotation { get; set; }
    public float Opacity { get; set; } = 1f;

    public string? FontName { get; set; }
    public float FontSize { get; set; } = 42;
    public string? ColorHex { get; set; } = "#FFFFFF";
    public string? StrokeColorHex { get; set; } = "#000000";
    public float StrokeThickness { get; set; } = 0;
    public bool Bold { get; set; }
    public TextAlignmentDto TextAlign { get; set; } = TextAlignmentDto.Center;

    public int ZIndex { get; set; }
}

public enum CardLayerType
{
    Image = 1,
    Text = 2
}

public enum TextAlignmentDto
{
    Left = 1,
    Center = 2,
    Right = 3
}
