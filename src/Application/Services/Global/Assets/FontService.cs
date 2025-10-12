using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Logging;
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
    private readonly object _syncRoot = new();

    public void LoadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            foreach (var fontType in FontPaths.Keys)
                EnsureFontLoaded(fontType);
            return;
        }

        EnsureFontLoaded(type);
    }

    public void UnloadFonts(FontType type = FontType.All)
    {
        if (type == FontType.All)
        {
            Font[] fontsToRelease;
            lock (_syncRoot)
            {
                fontsToRelease = _fonts.Values.Where(font => font != null).Distinct().ToArray();
                _fonts.Clear();
            }

            foreach (var font in fontsToRelease)
                Addressables.Release(font);

            return;
        }

        Font toRelease;
        lock (_syncRoot)
        {
            _fonts.Remove(type, out toRelease);
        }

        if (toRelease != null)
            Addressables.Release(toRelease);
    }

    public Font GetFont(FontType type)
    {
        lock (_syncRoot)
        {
            if (!_fonts.TryGetValue(type, out var font) || font == null)
                throw new ArgumentException($"Font {type} is not loaded.", nameof(type));
            return font;
        }
    }

    private void EnsureFontLoaded(FontType type)
    {
        lock (_syncRoot)
        {
            if (_fonts.TryGetValue(type, out var cached) && cached != null)
                return;
        }

        if (!FontPaths.TryGetValue(type, out var path) || string.IsNullOrWhiteSpace(path))
        {
            Logger?.Warn($"No addressable path configured for font [{type}].");
            return;
        }

        try
        {
            var handle = Addressables.LoadAssetAsync<Font>(path);
            var fontAsset = handle.WaitForCompletion();
            if (fontAsset == null)
                throw new InvalidOperationException($"Addressables returned null for '{path}'.");

            lock (_syncRoot)
            {
                _fonts[type] = fontAsset;
            }
        }
        catch (Exception ex)
        {
            Logger?.Error($"Failed to load font [{type}] from address '{path}'.");
            Logger?.Error(ex);
            lock (_syncRoot)
            {
                _fonts.Remove(type);
            }
        }
    }

    [UsedImplicitly] [Inject] public ILogger<FontService> Logger { get; set; }
}