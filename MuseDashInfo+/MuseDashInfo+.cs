using MelonLoader;
using System.Linq;
using System;

using MDIP.Managers;
using MDIP.Modules.Configs;
using MDIP.Modules;
using MDIP.Utils;
using MDIP.Patches;

namespace MDIP;

internal static class ModBuildInfo
{
    public const string NAME = "Info+";
    public const string DESCRIPTION = "Displays additional in-game infos";
    public const string AUTHOR = "KARPED1EM";
    public const string VERSION = "2.1.0";
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
        });
        ConfigManager.Instance.SaveConfig(ConfigName.MainConfigs, Configs.Main);

        ConfigManager.Instance.RegisterModule(ConfigName.AdvancedConfigs, "AdvancedConfigs.yml");
        ConfigManager.Instance.SaveConfig(ConfigName.AdvancedConfigs, Configs.Advanced);

        ConfigManager.Instance.RegisterModule(ConfigName.TextFieldLowerLeftConfigs, "TextFieldLowerLeftConfigs.yml");
        ConfigManager.Instance.SaveConfig(ConfigName.TextFieldLowerLeftConfigs, Configs.TextFieldLowerLeft);

        ConfigManager.Instance.RegisterModule(ConfigName.TextFieldLowerRightConfigs, "TextFieldLowerRightConfigs.yml");
        ConfigManager.Instance.SaveConfig(ConfigName.TextFieldLowerRightConfigs, Configs.TextFieldLowerRight);

        ConfigManager.Instance.RegisterModule(ConfigName.TextFieldScoreBelowConfigs, "TextFieldScoreBelowConfigs.yml");
        ConfigManager.Instance.SaveConfig(ConfigName.TextFieldScoreBelowConfigs, Configs.TextFieldScoreBelow);

        ConfigManager.Instance.RegisterModule(ConfigName.TextFieldScoreRightConfigs, "TextFieldScoreRightConfigs.yml");
        ConfigManager.Instance.SaveConfig(ConfigName.TextFieldScoreRightConfigs, Configs.TextFieldScoreRight);

        ConfigManager.Instance.RegisterModule(ConfigName.TextFieldUpperLeftConfigs, "TextFieldUpperLeftConfigs.yml");
        ConfigManager.Instance.SaveConfig(ConfigName.TextFieldUpperLeftConfigs, Configs.TextFieldUpperLeft);

        ConfigManager.Instance.RegisterModule(ConfigName.TextFieldUpperRightConfigs, "TextFieldUpperRightConfigs.yml");
        ConfigManager.Instance.SaveConfig(ConfigName.TextFieldUpperRightConfigs, Configs.TextFieldUpperRight);
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "GameMain":
                break;

            case "Welcome":
            case "UISystem_PC":
                ConfigManager.Instance.ActivateWatcher();
                break;

            default:

                if (Helper.OutputNoteRecordsToDesktop)
                    NoteRecordManager.Reset();

                PnlBattleGameStartPatch.Reset();
                GameStatsManager.Reset();
                TextObjManager.Reset();
                GameUtils.Reset();
                FontUtils.UnloadFonts();

                break;
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (GameStatsManager.IsInGame)
            PnlBattleGameStartPatch.CheckAndZoom();
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
