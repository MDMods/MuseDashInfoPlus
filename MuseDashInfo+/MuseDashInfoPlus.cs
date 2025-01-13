using MelonLoader;
using MuseDashInfoPlus.Manager;
using MuseDashInfoPlus.Utils;

namespace MuseDashInfoPlus;

internal static class ModBuildInfo
{
    public const string NAME = "Info Plus";
    public const string DESCRIPTION = "Displays additional in-game info";
    public const string AUTHOR = "KARPED1EM";
    public const string VERSION = "1.0.0";
    public const string REPO_LINK = "https://github.com/KARPED1EM/MuseDashInfoPlus";
}

public class InfoPlusMod : MelonMod
{
    public static InfoPlusMod instance { get; private set; }

    public override void OnInitializeMelon()
    {
        instance = this;

        ConfigManager.Init();
        ConfigManager.Load();
        ConfigManager.ConstractTextFormats();
        ConfigManager.Save();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "GameMain")
        {
            GameStatsUtils.Reload();
        }
        else
        {
            GameStatsUtils.Reset();
            CountsTextManager.Reset();
            UITextUtils.UnloadFonts(TextFontType.SnapsTaste);
        }
    }

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        MelonPreferences.Save();
    }
}