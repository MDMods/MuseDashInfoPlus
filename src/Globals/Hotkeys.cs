using MDIP.Core.Domain.Configs;

namespace MDIP.Globals;

// The UI-toggle hotkey: parsed from config once at startup (Init) and re-parsed on a MainConfigs
// change. CheckToggleTriggered is a pure poll — it no longer carries any lazy initialization, so the
// visibility default is decided independently of the in-battle zoom state.
// ReSharper disable StringLiteralTypo
internal static class Hotkeys
{
    private static readonly object Sync = new();
    private static bool _enabled;
    private static HotkeyCombination _toggle;

    public static void Init()
    {
        lock (Sync)
        {
            BindFrom(SafeMain());
        }

        Config.RegisterUpdateCallback<MainConfigs>(nameof(MainConfigs), _ =>
        {
            try
            {
                RebindFromConfig();
            }
            catch (Exception ex)
            {
                Log.Warn($"Rebind hotkey failed: {ex.Message}");
            }
        });
    }

    public static bool CheckToggleTriggered()
    {
        if (!_enabled || _toggle == null)
            return false;

        try
        {
            return _toggle.IsTriggered();
        }
        catch (Exception ex)
        {
            Log.Warn($"Hotkey check failed: {ex.Message}");
            return false;
        }
    }

    public static void RebindFromConfig()
    {
        lock (Sync)
        {
            BindFrom(SafeMain());
        }
    }

    private static MainConfigs SafeMain()
    {
        try
        {
            return Config.Main;
        }
        catch
        {
            return null;
        }
    }

    private static void BindFrom(MainConfigs cfg)
    {
        if (cfg == null)
        {
            _enabled = true;
            _toggle = HotkeyCombination.TryParse("F10");
            return;
        }

        _enabled = cfg.EnableUiToggleHotkey;
        var raw = string.IsNullOrWhiteSpace(cfg.UiToggleHotkey) ? "F10" : cfg.UiToggleHotkey.Trim();

        var parsed = HotkeyCombination.TryParse(raw);
        if (parsed == null)
        {
            Log.Warn($"Invalid hotkey '{raw}', fallback to F10.");
            parsed = HotkeyCombination.TryParse("F10");
        }

        _toggle = parsed;
    }

    private sealed class HotkeyCombination
    {
        private readonly KeyCode _main;
        private readonly List<KeyCode[]> _modifierGroups = new();

        private HotkeyCombination(KeyCode main)
        {
            _main = main;
        }

        public static HotkeyCombination TryParse(string str)
        {
            try
            {
                var tokens = str.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tokens.Length == 0)
                    return null;

                var mods = new List<string>();
                string mainToken = null;

                foreach (var token in tokens)
                {
                    var t = token.Trim();
                    if (IsModifierToken(t))
                        mods.Add(t);
                    else
                        mainToken = t;
                }

                if (string.IsNullOrWhiteSpace(mainToken))
                {
                    if (tokens.Length == 1 && IsModifierToken(tokens[0]))
                        return null;
                    mainToken = tokens.Last();
                }

                if (!TryMapKey(mainToken, out var mainKey))
                    return null;

                var combo = new HotkeyCombination(mainKey);
                foreach (var group in mods.Select(MapModifierToKeyGroup).Where(group => group is { Length: > 0 }))
                    combo._modifierGroups.Add(group);

                return combo;
            }
            catch (Exception ex)
            {
                Log.Warn($"Parse hotkey '{str}' failed: {ex.Message}");
                return null;
            }
        }

        public bool IsTriggered()
            => _modifierGroups.Select(group => group.Any(UnityEngine.Input.GetKey)).All(anyDown => anyDown) && UnityEngine.Input.GetKeyDown(_main);

        private static bool IsModifierToken(string token)
        {
            var t = token.ToLowerInvariant();
            return t is "ctrl" or "control" or "lctrl" or "leftctrl" or "rctrl" or "rightctrl"
                or "shift" or "lshift" or "leftshift" or "rshift" or "rightshift"
                or "alt" or "lalt" or "leftalt" or "ralt" or "rightalt";
        }

        private static KeyCode[] MapModifierToKeyGroup(string token)
        {
            var t = token.ToLowerInvariant();
            return t switch
            {
                "ctrl" or "control" => [KeyCode.LeftControl, KeyCode.RightControl],
                "lctrl" or "leftctrl" => [KeyCode.LeftControl],
                "rctrl" or "rightctrl" => [KeyCode.RightControl],
                "shift" => [KeyCode.LeftShift, KeyCode.RightShift],
                "lshift" or "leftshift" => [KeyCode.LeftShift],
                "rshift" or "rightshift" => [KeyCode.RightShift],
                "alt" => [KeyCode.LeftAlt, KeyCode.RightAlt],
                "lalt" or "leftalt" => [KeyCode.LeftAlt],
                "ralt" or "rightalt" => [KeyCode.RightAlt],
                _ => []
            };
        }

        private static bool TryMapKey(string token, out KeyCode key)
        {
            var t = token.Trim();

            if (Enum.TryParse(t, true, out key))
                return true;

            if (t.Length == 1)
            {
                var ch = char.ToUpperInvariant(t[0]);
                switch (ch)
                {
                    case >= 'A' and <= 'Z':
                        key = (KeyCode)Enum.Parse(typeof(KeyCode), ch.ToString(), true);
                        return true;
                    case >= '0' and <= '9':
                        key = (KeyCode)Enum.Parse(typeof(KeyCode), $"Alpha{ch}", true);
                        return true;
                }
            }

            var lower = t.ToLowerInvariant();

            if (lower.StartsWith("num") && lower.Length == 4 && char.IsDigit(lower[3]))
            {
                key = (KeyCode)Enum.Parse(typeof(KeyCode), $"Keypad{lower[3]}", true);
                return true;
            }

            if (lower.StartsWith("kp") && lower.Length == 3 && char.IsDigit(lower[2]))
            {
                key = (KeyCode)Enum.Parse(typeof(KeyCode), $"Keypad{lower[2]}", true);
                return true;
            }

            if (!lower.StartsWith("f") || !int.TryParse(lower[1..], out var fn) || fn is < 1 or > 24)
                return false;

            key = (KeyCode)Enum.Parse(typeof(KeyCode), $"F{fn}", true);
            return true;
        }
    }
}
