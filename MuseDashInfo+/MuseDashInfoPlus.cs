using MelonLoader;

using MuseDashInfoPlus.Manager;
using MuseDashInfoPlus.Patches;
using MuseDashInfoPlus.Utils;
using System.Linq;

namespace MuseDashInfoPlus;

internal static class ModBuildInfo
{
    public const string NAME = "Info+";
    public const string DESCRIPTION = "Displays additional in-game infos";
    public const string AUTHOR = "KARPED1EM";
    public const string VERSION = "1.2.0";
    public const string REPO_LINK = "https://github.com/KARPED1EM/MuseDashInfoPlus";
}

public class MuseDashInfoPlus : MelonMod
{
    public static MuseDashInfoPlus Instance { get; private set; }

    public static bool IsSongDescLoaded { get; private set; }

    public override void OnInitializeMelon()
    {
        Instance = this;
    }

    public override void OnLateInitializeMelon()
    {
        IsSongDescLoaded = RegisteredMelons.Any(mod => mod.MelonAssembly.Assembly.FullName.TrimStart().StartsWith("SongDesc"));

        ConfigManager.Init();
        ConfigManager.Load();
        ConfigManager.ConstractTextFormats();
        ConfigManager.Save();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "GameMain":
                GameStatsUtils.Reload();
                break;

            default:
                GameStatsUtils.Reset();
                StatsTextManager.Reset();
                FontUtils.UnloadFonts(TextFontType.SnapsTaste);
                PnlBattleGameStartPatch.Reset();
                break;
        }
    }
}