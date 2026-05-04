using PngCardGenerator.Api.Models;
using PngCardGenerator.Api.Rendering;

namespace PngCardGenerator.Api.Services;

public sealed class CardRendererService
{
    private readonly RenderPipeline _renderPipeline;

    public CardRendererService(RenderPipeline renderPipeline)
    {
        _renderPipeline = renderPipeline;
    }

    public Task<byte[]> RenderAsync(CardRenderRequest request, CancellationToken cancellationToken)
    {
        Normalize(request);
        return _renderPipeline.ExecuteAsync(request, cancellationToken);
    }

    private static void Normalize(CardRenderRequest request)
    {
        var z = 0;

        foreach (var layer in request.Layers.OrderBy(x => x.ZIndex))
        {
            if (string.IsNullOrWhiteSpace(layer.Id))
                layer.Id = Guid.NewGuid().ToString("N");

            if (layer.Width < 0)
                layer.Width = 0;

            if (layer.Height < 0)
                layer.Height = 0;

            layer.Opacity = Math.Clamp(layer.Opacity, 0f, 1f);

            if (layer.ZIndex == 0)
                layer.ZIndex = z;

            z++;
        }
    }
}
