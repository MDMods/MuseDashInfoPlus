using MelonLoader;
using System.Linq;
using System;

using MDIP.Managers;
using MDIP.Modules.Configs;
using MDIP.Patches;
using MDIP.Utils;
using MDIP.Modules;

namespace MDIP;

internal static class ModBuildInfo
{
    public const string NAME = "Info+";
    public const string DESCRIPTION = "Displays additional in-game infos";
    public const string AUTHOR = "KARPED1EM";
    public const string VERSION = "1.4.0";
    public const string REPO_LINK = "https://github.com/MDMods/MuseDashInfoPlus";
}

/// <summary>
/// MDIP (MuseDashInfoPlus) main mod class
/// </summary>
public class MDIPMod : MelonMod
{
    public static MDIPMod Instance { get; private set; }

    public static bool IsSongDescLoaded { get; private set; }

    public override void OnInitializeMelon() => Instance = this;

    public override void OnLateInitializeMelon()
    {
        IsSongDescLoaded = RegisteredMelons.Any(mod => mod.MelonAssembly.Assembly.FullName.TrimStart().StartsWith("SongDesc"));

        ConfigManager.Instance.RegisterModule(ConfigName.MainConfigs, "MainConfigs.yml");
        var mainConfigModule = ConfigManager.Instance.GetModule(ConfigName.MainConfigs);
        mainConfigModule.RegisterUpdateCallback<MainConfigs>(cfg =>
        {
            Melon<MDIPMod>.Logger.Warning("Main configs updated!");
            TextDataManager.ConstractTextFormats();
        });
        TextDataManager.ConstractTextFormats();
        ConfigManager.Instance.SaveConfig(ConfigName.MainConfigs, Configs.Main);
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "GameMain":
                break;

            default:
                GameStatsManager.Reset();
                TextObjManager.Reset();
#if DEBUG
                NoteRecordManager.Reset();
#endif
                PnlBattleGameStartPatch.Reset();
                FontUtils.UnloadFonts(TextFontType.SnapsTaste);
                break;
        }
    }

    private static int lastUpdateSecond = -1;
    public override void OnLateUpdate()
    {
        base.OnLateUpdate();
        if (lastUpdateSecond == DateTime.Now.Second || !GameStatsManager.IsInGame) return;
        lastUpdateSecond = DateTime.Now.Second;

        GameStatsManager.CheckMashing();
        TextObjManager.UpdateAllText();
    }
}
