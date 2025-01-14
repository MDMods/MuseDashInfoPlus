using Il2CppAssets.Scripts.UI.Panels;
using MelonLoader;
using MuseDashInfoPlus.Manager;
using MuseDashInfoPlus.Patches;
using MuseDashInfoPlus.Utils;

namespace MuseDashInfoPlus;

internal static class ModBuildInfo
{
    public const string NAME = "Info Plus";
    public const string DESCRIPTION = "Displays additional in-game info";
    public const string AUTHOR = "KARPED1EM";
    public const string VERSION = "1.1.0";
    public const string REPO_LINK = "https://github.com/KARPED1EM/MuseDashInfoPlus";
}

public class InfoPlusMod : MelonMod
{
    public static InfoPlusMod instance { get; private set; }

    public override void OnInitializeMelon()
    {
        instance = this;
    }

    public override void OnLateInitializeMelon()
    {
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