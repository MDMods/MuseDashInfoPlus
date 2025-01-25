using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

using MDIP.Modules;

namespace MDIP.Utils;

public static class FontUtils
{
    private static readonly Dictionary<FontType, Font> _fonts = new();
    private static readonly Dictionary<FontType, string> _fontPaths = new()
    {
        { FontType.SnapsTaste, "Snaps Taste" },
        { FontType.LatoRegular, "Lato-Regular" },
        { FontType.Normal, "Normal" }
    };

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
        if (_fontPaths.TryGetValue(type, out string path))
            _fonts[type] = Addressables.LoadAssetAsync<Font>(path).WaitForCompletion();
    }

    public static void UnloadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            foreach (var curFont in _fonts.Values)
                Addressables.Release(curFont);
            _fonts.Clear();
            return;
        }

        if (_fonts.TryGetValue(type, out Font font))
        {
            Addressables.Release(font);
            _fonts.Remove(type);
        }
    }

    public static Font GetFont(FontType type)
        => _fonts.TryGetValue(type, out Font font) ? font : throw new ArgumentException($"Font {type} not loaded");
}