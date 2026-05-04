using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PngCardGenerator.Api.Data;
using PngCardGenerator.Api.Models;

namespace PngCardGenerator.Api.Services;

public sealed class CardTemplateService
{
    private readonly CardGeneratorDbContext _dbContext;

    public CardTemplateService(CardGeneratorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CardTemplateSummaryDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.CardTemplates
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(x => new CardTemplateSummaryDto
            {
                Id = x.Id,
                Name = x.Name,
                Width = x.Width,
                Height = x.Height,
                LayerCount = x.Layers.Count,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CardTemplateDetailsDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await LoadTemplateAsync(id, cancellationToken);
        return template is null ? null : MapDetails(template);
    }

    public async Task<CardTemplateDetailsDto> CreateAsync(CardTemplateUpsertRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var template = new CardTemplate
        {
            Name = request.Name.Trim(),
            Width = request.Width,
            Height = request.Height,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Layers = await MapLayersAsync(request.Layers, cancellationToken)
        };

        _dbContext.CardTemplates.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapDetails(template);
    }

    public async Task<CardTemplateDetailsDto?> UpdateAsync(Guid id, CardTemplateUpsertRequest request, CancellationToken cancellationToken)
    {
        var template = await _dbContext.CardTemplates
            .Include(x => x.Layers)
            .ThenInclude(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (template is null)
            return null;

        var existingLayers = template.Layers.ToList();
        if (existingLayers.Count > 0)
            _dbContext.CardTemplateLayers.RemoveRange(existingLayers);

        template.Name = request.Name.Trim();
        template.Width = request.Width;
        template.Height = request.Height;
        template.UpdatedAtUtc = DateTime.UtcNow;
        template.Layers = await MapLayersAsync(request.Layers, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await RemoveOrphanAssetsAsync(cancellationToken);

        return MapDetails(template);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await _dbContext.CardTemplates
            .Include(x => x.Layers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (template is null)
            return false;

        _dbContext.CardTemplates.Remove(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await RemoveOrphanAssetsAsync(cancellationToken);
        return true;
    }

    public async Task<CardRenderRequest?> BuildRenderRequestAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var template = await LoadTemplateAsync(templateId, cancellationToken);
        if (template is null)
            return null;

        return new CardRenderRequest
        {
            Width = template.Width,
            Height = template.Height,
            Layers = template.Layers
                .OrderBy(x => x.ZIndex)
                .ThenBy(x => x.ClientLayerId)
                .Select(MapLayer)
                .ToList()
        };
    }

    public async Task RegisterGeneratedCardAsync(Guid templateId, string templateName, CardRenderRequest request, CancellationToken cancellationToken)
    {
        var generatedCard = new GeneratedCard
        {
            CardTemplateId = templateId,
            Name = templateName,
            Width = request.Width,
            Height = request.Height,
            LayerCount = request.Layers.Count,
            RequestSnapshotJson = JsonSerializer.Serialize(request),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.GeneratedCards.Add(generatedCard);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<CardTemplate?> LoadTemplateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.CardTemplates
            .AsNoTracking()
            .Include(x => x.Layers.OrderBy(layer => layer.ZIndex))
            .ThenInclude(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private async Task<List<CardTemplateLayer>> MapLayersAsync(IEnumerable<CardLayerDto> layers, CancellationToken cancellationToken)
    {
        var mapped = new List<CardTemplateLayer>();

        foreach (var layer in layers.OrderBy(x => x.ZIndex).ThenBy(x => x.Id))
        {
            mapped.Add(new CardTemplateLayer
            {
                ClientLayerId = string.IsNullOrWhiteSpace(layer.Id) ? Guid.NewGuid().ToString("N") : layer.Id,
                Type = layer.Type,
                Name = string.IsNullOrWhiteSpace(layer.Name) ? layer.Text ?? layer.Type.ToString() : layer.Name,
                Asset = await ResolveAssetAsync(layer.ImageBase64, cancellationToken),
                Text = layer.Text,
                X = layer.X,
                Y = layer.Y,
                Width = layer.Width,
                Height = layer.Height,
                Rotation = layer.Rotation,
                Opacity = Math.Clamp(layer.Opacity, 0f, 1f),
                FontName = layer.FontName,
                FontSize = layer.FontSize,
                ColorHex = layer.ColorHex,
                StrokeColorHex = layer.StrokeColorHex,
                StrokeThickness = layer.StrokeThickness,
                Bold = layer.Bold,
                TextAlign = layer.TextAlign,
                ZIndex = layer.ZIndex
            });
        }

        return mapped;
    }

    private async Task<TemplateAsset?> ResolveAssetAsync(string? imageBase64, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imageBase64))
            return null;

        var (mimeType, normalizedBase64) = NormalizeImagePayload(imageBase64);
        var hash = Convert.ToHexString(SHA256.HashData(Convert.FromBase64String(normalizedBase64)));

        var existing = await _dbContext.TemplateAssets.FirstOrDefaultAsync(x => x.Sha256 == hash, cancellationToken);
        if (existing is not null)
            return existing;

        return new TemplateAsset
        {
            MimeType = mimeType,
            Base64Data = normalizedBase64,
            Sha256 = hash,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private async Task RemoveOrphanAssetsAsync(CancellationToken cancellationToken)
    {
        var orphanAssets = await _dbContext.TemplateAssets
            .Where(x => !x.Layers.Any())
            .ToListAsync(cancellationToken);

        if (orphanAssets.Count == 0)
            return;

        _dbContext.TemplateAssets.RemoveRange(orphanAssets);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static CardTemplateDetailsDto MapDetails(CardTemplate template)
    {
        return new CardTemplateDetailsDto
        {
            Id = template.Id,
            Name = template.Name,
            Width = template.Width,
            Height = template.Height,
            LayerCount = template.Layers.Count,
            CreatedAtUtc = template.CreatedAtUtc,
            UpdatedAtUtc = template.UpdatedAtUtc,
            Layers = template.Layers
                .OrderBy(x => x.ZIndex)
                .ThenBy(x => x.ClientLayerId)
                .Select(MapLayer)
                .ToList()
        };
    }

    private static CardLayerDto MapLayer(CardTemplateLayer layer)
    {
        return new CardLayerDto
        {
            Id = layer.ClientLayerId,
            Type = layer.Type,
            Name = layer.Name,
            ImageBase64 = layer.Asset is null ? null : $"data:{layer.Asset.MimeType};base64,{layer.Asset.Base64Data}",
            Text = layer.Text,
            X = layer.X,
            Y = layer.Y,
            Width = layer.Width,
            Height = layer.Height,
            Rotation = layer.Rotation,
            Opacity = layer.Opacity,
            FontName = layer.FontName,
            FontSize = layer.FontSize,
            ColorHex = layer.ColorHex,
            StrokeColorHex = layer.StrokeColorHex,
            StrokeThickness = layer.StrokeThickness,
            Bold = layer.Bold,
            TextAlign = layer.TextAlign,
            ZIndex = layer.ZIndex
        };
    }

    private static (string MimeType, string Base64Data) NormalizeImagePayload(string value)
    {
        var trimmed = value.Trim();
        var commaIndex = trimmed.IndexOf(',');

        if (commaIndex < 0)
            return ("image/png", trimmed);

        var header = trimmed[..commaIndex];
        var mimeType = "image/png";

        if (header.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var mediaType = header[5..];
            var separatorIndex = mediaType.IndexOf(';');
            if (separatorIndex > 0)
                mimeType = mediaType[..separatorIndex];
        }

        return (mimeType, trimmed[(commaIndex + 1)..]);
    }
}
