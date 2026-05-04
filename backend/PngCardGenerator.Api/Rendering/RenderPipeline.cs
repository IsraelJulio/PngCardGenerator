using PngCardGenerator.Api.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PngCardGenerator.Api.Rendering;

public sealed class RenderPipeline
{
    private readonly ImageCacheService _imageCacheService;
    private readonly FontService _fontService;

    public RenderPipeline(ImageCacheService imageCacheService, FontService fontService)
    {
        _imageCacheService = imageCacheService;
        _fontService = fontService;
    }

    public async Task<byte[]> ExecuteAsync(CardRenderRequest request, CancellationToken cancellationToken)
    {
        using var canvas = new Image<Rgba32>(request.Width, request.Height, Color.Transparent);

        var orderedLayers = request.Layers
            .OrderBy(x => x.ZIndex)
            .ThenBy(x => x.Id)
            .ToList();

        foreach (var layer in orderedLayers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (layer.Opacity <= 0)
                continue;

            switch (layer.Type)
            {
                case CardLayerType.Image:
                    DrawImageLayer(canvas, layer);
                    break;

                case CardLayerType.Text:
                    DrawTextLayer(canvas, layer);
                    break;
            }
        }

        await using var output = new MemoryStream();
        await canvas.SaveAsync(output, new PngEncoder
        {
            ColorType = PngColorType.RgbWithAlpha,
            CompressionLevel = PngCompressionLevel.BestCompression
        }, cancellationToken);

        return output.ToArray();
    }

    private void DrawImageLayer(Image<Rgba32> canvas, CardLayerDto layer)
    {
        if (string.IsNullOrWhiteSpace(layer.ImageBase64))
            return;

        using var image = _imageCacheService.GetDecodedImage(layer.ImageBase64);

        var targetWidth = Math.Max(1, (int)Math.Round(layer.Width));
        var targetHeight = Math.Max(1, (int)Math.Round(layer.Height));

        image.Mutate(ctx =>
        {
            ctx.Resize(targetWidth, targetHeight);

            if (Math.Abs(layer.Rotation) > 0.01f)
                ctx.Rotate(layer.Rotation);
        });

        canvas.Mutate(ctx =>
        {
            ctx.DrawImage(
                image,
                new Point((int)Math.Round(layer.X), (int)Math.Round(layer.Y)),
                Math.Clamp(layer.Opacity, 0f, 1f));
        });
    }

    private void DrawTextLayer(Image<Rgba32> canvas, CardLayerDto layer)
    {
        if (string.IsNullOrWhiteSpace(layer.Text))
            return;

        var font = _fontService.GetFont(layer.FontName, layer.FontSize, layer.Bold);
        var color = SafeColor(layer.ColorHex, Color.White);
        var stroke = SafeColor(layer.StrokeColorHex, Color.Black);

        var options = new RichTextOptions(font)
        {
            Origin = new PointF(layer.X, layer.Y),
            HorizontalAlignment = layer.TextAlign switch
            {
                TextAlignmentDto.Left => HorizontalAlignment.Left,
                TextAlignmentDto.Right => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Center
            },
            WrappingLength = layer.Width > 0 ? layer.Width : 0
        };

        canvas.Mutate(ctx =>
        {
            if (layer.StrokeThickness > 0)
            {
                ctx.DrawText(options, layer.Text, Pens.Solid(stroke, layer.StrokeThickness));
            }

            ctx.DrawText(options, layer.Text, color);
        });
    }

    private static Color SafeColor(string? hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return fallback;

        try
        {
            return Color.ParseHex(hex);
        }
        catch
        {
            return fallback;
        }
    }
}
