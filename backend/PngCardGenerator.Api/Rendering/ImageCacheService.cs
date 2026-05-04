using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PngCardGenerator.Api.Rendering;

public sealed class ImageCacheService
{
    private readonly IMemoryCache _cache;

    public ImageCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Image<Rgba32> GetDecodedImage(string base64)
    {
        var normalized = NormalizeBase64(base64);
        var key = $"img:{normalized.Length}:{normalized.GetHashCode()}";

        var cachedBytes = _cache.GetOrCreate(key, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(20);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
            return Convert.FromBase64String(normalized);
        })!;

        return Image.Load<Rgba32>(cachedBytes);
    }

    private static string NormalizeBase64(string value)
    {
        var commaIndex = value.IndexOf(',');
        return commaIndex >= 0 ? value[(commaIndex + 1)..] : value;
    }
}
