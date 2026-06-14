using MDIP.Battle;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Updates;
using MDIP.Globals;

namespace MDIP.Presentation;

// ReSharper disable once InconsistentNaming
public class MDIPMod : MelonMod
{
    public static bool IsSongDescLoaded { get; private set; }

    private static readonly (Type type, string name)[] ConfigModules =
    [
        (typeof(MainConfigs), nameof(MainConfigs)),
        (typeof(AdvancedConfigs), nameof(AdvancedConfigs)),
        (typeof(TextFieldLowerLeftConfigs), nameof(TextFieldLowerLeftConfigs)),
        (typeof(TextFieldLowerRightConfigs), nameof(TextFieldLowerRightConfigs)),
        (typeof(TextFieldScoreBelowConfigs), nameof(TextFieldScoreBelowConfigs)),
        (typeof(TextFieldScoreRightConfigs), nameof(TextFieldScoreRightConfigs)),
        (typeof(TextFieldUpperLeftConfigs), nameof(TextFieldUpperLeftConfigs)),
        (typeof(TextFieldUpperRightConfigs), nameof(TextFieldUpperRightConfigs))
    ];

    public override void OnInitializeMelon()
    {
        _ = Task.Run(CheckForUpdatesAsync);
    }

    public override void OnLateInitializeMelon()
    {
        IsSongDescLoaded = RegisteredMelons.Any(mod => mod?.MelonAssembly?.Assembly?.FullName?.TrimStart().StartsWith("SongDesc") ?? false);

        Config.Init();
        foreach (var (type, name) in ConfigModules)
            RegisterAndSaveConfig(type, name);

        Config.ActivateWatcher();

        // Initialize the visibility toggle from config ONCE, up front — independent of the in-battle
        // zoom state. (The old lazy init ran during the hotkey poll, which never fired when another
        // mod held the battle frozen, leaving the overlay stuck hidden.)
        RuntimeData.InitDesiredUiVisible(Config.Main.UiVisibleByDefault);

        Hotkeys.Init();
        RegisterConfigChangeHandlers();

        Log.Info($"{ModBuildInfo.Name} has loaded correctly!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "Loading":
                BattleController.EndSession();
                Fonts.UnloadFonts();
                break;
        }
    }

    public override void OnFixedUpdate() => BattleController.OnFixedUpdate();

    public override void OnLateUpdate() => BattleController.OnLateUpdate();

    private static void RegisterAndSaveConfig(Type configType, string moduleName)
    {
        var fileName = $"{moduleName}.yml";
        Config.RegisterModule(moduleName, fileName);

        var get = typeof(Config).GetMethod(nameof(Config.GetConfig))!.MakeGenericMethod(configType);
        var config = get.Invoke(null, [moduleName]);
        typeof(Config).GetMethod(nameof(Config.SaveConfig))!.MakeGenericMethod(configType)
            .Invoke(null, [moduleName, config]);
    }

    private static void RegisterConfigChangeHandlers()
    {
        // Any config change clears the text caches + recomputes constants, and asks the live overlay
        // to re-apply. Registered once for every module; the hotkey rebind handler is registered
        // separately by Hotkeys.Init.
        Config.RegisterUpdateCallback<MainConfigs>(nameof(MainConfigs), _ => OnAnyConfigChanged());
        Config.RegisterUpdateCallback<AdvancedConfigs>(nameof(AdvancedConfigs), _ => OnAnyConfigChanged());
        Config.RegisterUpdateCallback<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs), _ => OnAnyConfigChanged());
        Config.RegisterUpdateCallback<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs), _ => OnAnyConfigChanged());
        Config.RegisterUpdateCallback<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs), _ => OnAnyConfigChanged());
        Config.RegisterUpdateCallback<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs), _ => OnAnyConfigChanged());
        Config.RegisterUpdateCallback<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs), _ => OnAnyConfigChanged());
        Config.RegisterUpdateCallback<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs), _ => OnAnyConfigChanged());
    }

    private static void OnAnyConfigChanged()
    {
        TextData.QueueRefresh();
        BattleController.QueueConfigApply();
    }

    private static async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateInfo = await Updates.GetUpdateInfoAsync();
            if (updateInfo == null)
            {
                Log.Error("Auto Updater: Failed to fetch update info.");
                return;
            }
            if (string.IsNullOrWhiteSpace(updateInfo.Hash))
                Log.Error("Auto Updater: Update hash is empty.");

            if (!Updates.IsUpdateAvailable(updateInfo))
            {
                Log.Info("Auto Updater: Already up to date.");
                return;
            }

            await HandleUpdateAsync(updateInfo);
        }
        catch (Exception ex)
        {
            Log.Error("Auto Updater: Update check failed.");
            Log.Error(ex.ToString());
        }
    }

    private static async Task HandleUpdateAsync(VersionInfo updateInfo)
    {
        var success = await Updates.ApplyUpdateAsync(updateInfo);
        if (success)
            Log.Warn("Auto Updater: Update successful!");
        else
            Log.Error("Auto Updater: Update failed!");
    }
}
