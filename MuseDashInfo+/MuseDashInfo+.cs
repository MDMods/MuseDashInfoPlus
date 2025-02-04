namespace MDIP;

/// <summary>
///     MDIP (MuseDashInfoPlus) main mod class
/// </summary>
public class MDIPMod : MelonMod
{
    private static int _lastUpdateSecond = -1;
    public static bool Reset { get; set; } = true;
    public static bool IsSongDescLoaded { get; private set; }

    public override void OnLateInitializeMelon()
    {
        IsSongDescLoaded = RegisteredMelons.Any(mod => mod?.MelonAssembly?.Assembly?.FullName?.TrimStart().StartsWith("SongDesc") ?? false);

        ConfigManager.Init();

        RegisterAndSaveConfig<MainConfigs>(nameof(MainConfigs));
        RegisterAndSaveConfig<AdvancedConfigs>(nameof(AdvancedConfigs));
        RegisterAndSaveConfig<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs));
        RegisterAndSaveConfig<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs));
        RegisterAndSaveConfig<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs));
        RegisterAndSaveConfig<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs));
        RegisterAndSaveConfig<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs));
        RegisterAndSaveConfig<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs));

        ConfigManager.ActivateWatcher();
        return;

        static void RegisterAndSaveConfig<T>(string moduleName) where T : ConfigBase, new()
        {
            var fileName = $"{moduleName}.yml";
            ConfigManager.RegisterModule(moduleName, fileName);
            ConfigManager.SaveConfig(moduleName, ConfigManager.GetConfig<T>(moduleName));
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "Loading" when !Reset:
                Reset = true;
                NoteRecordManager.Reset();
                PnlBattleGameStartPatch.Reset();
                GameStatsManager.Reset();
                TextObjManager.Reset();
                GameUtils.Reset();
                FontUtils.UnloadFonts();
                Melon<MDIPMod>.Logger.Warning("Reset");
                break;
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (GameStatsManager.IsInGame)
            PnlBattleGameStartPatch.CheckAndZoom();
    }

    public override void OnLateUpdate()
    {
        base.OnLateUpdate();

        if (_lastUpdateSecond == DateTime.Now.Second || !GameStatsManager.IsInGame)
            return;
        _lastUpdateSecond = DateTime.Now.Second;

        GameStatsManager.UpdateCurrentStats();
        GameStatsManager.CheckMashing();
        TextObjManager.UpdateAllText();
    }
}