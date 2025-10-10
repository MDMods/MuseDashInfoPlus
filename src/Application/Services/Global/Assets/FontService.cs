using MDIP.Core.Domain.Enums;
using UnityEngine.AddressableAssets;

namespace MDIP.Application.Services.Global.Assets;

public class FontService : IFontService
{
    private static readonly Dictionary<FontType, string> FontPaths = new()
    {
        { FontType.SnapsTaste, "Snaps Taste" },
        { FontType.LatoRegular, "Lato-Regular" },
        { FontType.LuckiestGuy, "LuckiestGuy-Regular" },
        { FontType.Normal, "Normal" }
    };

    private readonly Dictionary<FontType, Font> _fonts = new();

    public void LoadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            foreach (var fontType in FontPaths.Keys)
                LoadFont(fontType);
            return;
        }

        LoadFont(type);
    }

    public void UnloadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            foreach (var font in _fonts.Values)
                Addressables.Release(font);
            _fonts.Clear();
            return;
        }

        if (!_fonts.TryGetValue(type, out var fontAsset))
            return;

        Addressables.Release(fontAsset);
        _fonts.Remove(type);
    }

    public Font GetFont(FontType type) => !_fonts.TryGetValue(type, out var font) ? throw new ArgumentException($"Font {type} not loaded.") : font;

    private void LoadFont(FontType type)
    {
        if (_fonts.ContainsKey(type))
            return;

        if (FontPaths.TryGetValue(type, out var path))
            _fonts[type] = Addressables.LoadAssetAsync<Font>(path).WaitForCompletion();
    }
}