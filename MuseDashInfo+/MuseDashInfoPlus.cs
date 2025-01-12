using MelonLoader;
using MuseDashInfoPlus.Manager;
using MuseDashInfoPlus.Utils;

namespace MuseDashInfoPlus;

public class InfoPlusMod : MelonMod
{
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "GameMain")
        {
            GameCountsUtils.Reload();
        }
        else
        {
            GameCountsUtils.Reset();
            CountsTextManager.Reset();
            UITextUtils.UnloadFonts(TextFontType.SnapsTaste);
        }
    }
}