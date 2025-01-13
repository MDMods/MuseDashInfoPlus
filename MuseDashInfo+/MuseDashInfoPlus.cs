using MelonLoader;
using MuseDashInfoPlus.Manager;
using MuseDashInfoPlus.Utils;

namespace MuseDashInfoPlus;

public class InfoPlusMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        ConfigManager.Load();
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