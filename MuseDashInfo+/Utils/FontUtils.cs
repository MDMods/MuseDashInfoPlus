using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MDIP.Utils;

public static class FontUtils
{
    private static Font fontSnapsTaste;
    private static Font fontLatoRegular;

    public static void LoadFonts(TextFontType type = TextFontType.All)
    {
        if (type == TextFontType.SnapsTaste || type == TextFontType.All)
            fontSnapsTaste = Addressables.LoadAssetAsync<Font>("Snaps Taste").WaitForCompletion();
        if (type == TextFontType.LatoRegular || type == TextFontType.All)
            fontSnapsTaste = Addressables.LoadAssetAsync<Font>("Lato-Regular").WaitForCompletion();
    }

    public static void UnloadFonts(TextFontType type = TextFontType.All)
    {
        if (type == TextFontType.SnapsTaste || type == TextFontType.All)
            Addressables.Release(fontSnapsTaste);
        if (type == TextFontType.LatoRegular || type == TextFontType.All)
            Addressables.Release(fontLatoRegular);
    }

    public static Font GetFont(TextFontType type)
        => type switch
        {
            TextFontType.SnapsTaste => fontSnapsTaste,
            TextFontType.LatoRegular => fontLatoRegular,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}

public enum TextFontType
{
    SnapsTaste = 0,
    LatoRegular = 1,
    All = -1
}