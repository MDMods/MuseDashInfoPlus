using MelonLoader;
using System.Linq;

using MDIP.Manager;
using MDIP.Patches;
using MDIP.Utils;
using MDIP.Managers;

namespace MDIP;

internal static class ModBuildInfo
{
    public const string NAME = "Info+";
    public const string DESCRIPTION = "Displays additional in-game infos";
    public const string AUTHOR = "KARPED1EM";
    public const string VERSION = "1.3.0";
    public const string REPO_LINK = "https://github.com/MDMods/MuseDashInfoPlus";
}

/// <summary>
/// MDIP (MuseDashInfoPlus) main mod class
/// </summary>
public class MDIPMod : MelonMod
{
    public static MDIPMod Instance { get; private set; }

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
        ConfigManager.Save();
        ConfigManager.ConstractTextFormats();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "GameMain":
                break;

            default:
                GameStatsManager.Reset();
                StatsTextManager.Reset();
                NoteRecordManager.Reset();
                PnlBattleGameStartPatch.Reset();
                FontUtils.UnloadFonts(TextFontType.SnapsTaste);
                break;
        }
    }
}