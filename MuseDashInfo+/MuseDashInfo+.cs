namespace MDIP;

/// <summary>
///     MDIP (MuseDashInfoPlus) main mod class
/// </summary>
public class MDIPMod : MelonMod
{
    private static int _lastUpdateSecond = -1;
    public static bool Reset { get; set; } = true;
    public static bool IsSongDescLoaded { get; private set; }

    public override void OnInitializeMelon()
    {
        CheckForUpdatesAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
                Melon<MDIPMod>.Logger.Error($"Update check failed: {task.Exception}");
        });
    }

    private async static Task CheckForUpdatesAsync()
    {
        var updateManager = new UpdateManager();
        var updateInfo = await UpdateManager.GetUpdateInfo();
        if (updateInfo == null)
        {
            Melon<MDIPMod>.Logger.Error("Failed to fetch update info.");
            return;
        }

        if (!updateManager.CheckUpdate(updateInfo))
        {
            Melon<MDIPMod>.Logger.Msg("Already up to date.");
            return;
        }

        var success = await updateManager.Update(updateInfo);
        if (success) Melon<MDIPMod>.Logger.Warning("Auto update successful!");
        else Melon<MDIPMod>.Logger.Error("Auto update failed!");
    }

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
            case "UISystem_PC":
                GameStatsManager.Reset(true);
                break;
            case "Loading" when !Reset:
                Reset = true;
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

        PnlBattleGameStartPatch.CheckAndZoom();
    }

    public override void OnLateUpdate()
    {
        base.OnLateUpdate();

        if (!GameStatsManager.IsInGame || _lastUpdateSecond == DateTime.Now.Second)
            return;

        _lastUpdateSecond = DateTime.Now.Second;

        GameStatsManager.UpdateCurrentStats();
        TextObjManager.UpdateAllText();
    }
}