using SixLabors.Fonts;

namespace PngCardGenerator.Api.Rendering;

public sealed class FontService
{
    private readonly FontCollection _collection = new();
    private readonly FontFamily _fallbackFamily;

    public FontService()
    {
        var fontsFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts");

        if (Directory.Exists(fontsFolder))
        {
            foreach (var fontFile in Directory.GetFiles(fontsFolder, "*.ttf", SearchOption.AllDirectories))
            {
                try
                {
                    _collection.Add(fontFile);
                }
                catch
                {
                    // Ignore invalid font files.
                }
            }
        }

        _fallbackFamily = SystemFonts.Families.FirstOrDefault(x =>
            x.Name.Contains("Arial", StringComparison.OrdinalIgnoreCase) ||
            x.Name.Contains("DejaVu", StringComparison.OrdinalIgnoreCase) ||
            x.Name.Contains("Liberation", StringComparison.OrdinalIgnoreCase));

        if (_fallbackFamily.Name is null)
            _fallbackFamily = SystemFonts.Families.First();
    }

    public Font GetFont(string? preferredName, float size, bool bold)
    {
        if (!string.IsNullOrWhiteSpace(preferredName))
        {
            var family = _collection.Families.FirstOrDefault(x =>
                x.Name.Equals(preferredName, StringComparison.OrdinalIgnoreCase));

            if (family.Name is not null)
                return family.CreateFont(size, bold ? FontStyle.Bold : FontStyle.Regular);

            var systemFamily = SystemFonts.Families.FirstOrDefault(x =>
                x.Name.Equals(preferredName, StringComparison.OrdinalIgnoreCase));

            if (systemFamily.Name is not null)
                return systemFamily.CreateFont(size, bold ? FontStyle.Bold : FontStyle.Regular);
        }

        return _fallbackFamily.CreateFont(size, bold ? FontStyle.Bold : FontStyle.Regular);
    }
}
