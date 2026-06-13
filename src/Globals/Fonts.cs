using MDIP.Core.Domain.Enums;
using UnityEngine.AddressableAssets;

namespace MDIP.Globals;

// Loads/caches the addressable fonts the overlay uses. Idempotent: loading an already-loaded font
// is a no-op, so it is safe to call at the start of every battle.
internal static class Fonts
{
    private static readonly Dictionary<FontType, string> FontPaths = new()
    {
        { FontType.SnapsTaste, "Snaps Taste" },
        { FontType.LatoRegular, "Lato-Regular" },
        { FontType.LuckiestGuy, "LuckiestGuy-Regular" },
        { FontType.Normal, "Normal" }
    };

    private static readonly Dictionary<FontType, Font> Loaded = new();
    private static readonly object SyncRoot = new();

    public static void LoadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            foreach (var fontType in FontPaths.Keys)
                EnsureFontLoaded(fontType);
            return;
        }

        EnsureFontLoaded(type);
    }

    public static void UnloadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            Font[] fontsToRelease;
            lock (SyncRoot)
            {
                fontsToRelease = Loaded.Values.Where(font => font != null).Distinct().ToArray();
                Loaded.Clear();
            }

            foreach (var font in fontsToRelease)
                Addressables.Release(font);

            return;
        }

        Font toRelease;
        lock (SyncRoot)
        {
            Loaded.Remove(type, out toRelease);
        }

        if (toRelease != null)
            Addressables.Release(toRelease);
    }

    public static Font GetFont(FontType type)
    {
        lock (SyncRoot)
        {
            if (!Loaded.TryGetValue(type, out var font) || font == null)
                throw new ArgumentException($"Font {type} is not loaded.", nameof(type));
            return font;
        }
    }

    private static void EnsureFontLoaded(FontType type)
    {
        lock (SyncRoot)
        {
            if (Loaded.TryGetValue(type, out var cached) && cached != null)
                return;
        }

        if (!FontPaths.TryGetValue(type, out var path) || string.IsNullOrWhiteSpace(path))
        {
            Log.Warn($"No addressable path configured for font [{type}].");
            return;
        }

        try
        {
            var handle = Addressables.LoadAssetAsync<Font>(path);
            var fontAsset = handle.WaitForCompletion();
            if (fontAsset == null)
                throw new InvalidOperationException($"Addressables returned null for '{path}'.");

            lock (SyncRoot)
            {
                Loaded[type] = fontAsset;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load font [{type}] from address '{path}'.");
            Log.Error(ex);
            lock (SyncRoot)
            {
                Loaded.Remove(type);
            }
        }
    }
}
