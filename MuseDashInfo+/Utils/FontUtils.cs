using UnityEngine.AddressableAssets;

namespace MDIP.Utils;

public static class FontUtils
{
    private static readonly Dictionary<FontType, string> _fontPaths = new()
    {
        { FontType.SnapsTaste, "Snaps Taste" },
        { FontType.LatoRegular, "Lato-Regular" },
        { FontType.Normal, "Normal" }
    };

    private static Dictionary<FontType, Font> Fonts { get; } = new();

    public static void LoadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            foreach (var fontType in _fontPaths.Keys)
                LoadFont(fontType);
            return;
        }

        LoadFont(type);
    }

    private static void LoadFont(FontType type)
    {
        if (_fontPaths.TryGetValue(type, out var path))
            Fonts[type] = Addressables.LoadAssetAsync<Font>(path).WaitForCompletion();
    }

    public static void UnloadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            foreach (var curFont in Fonts.Values)
                Addressables.Release(curFont);
            Fonts.Clear();
            return;
        }

        if (!Fonts.TryGetValue(type, out var font))
            return;

        Addressables.Release(font);
        Fonts.Remove(type);
    }

    public static Font GetFont(FontType type)
        => Fonts.TryGetValue(type, out var font) ? font : throw new ArgumentException($"Font {type} not loaded");
}