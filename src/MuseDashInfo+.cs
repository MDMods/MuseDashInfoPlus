using MDIP.Services;

namespace MDIP;

public class MDIPMod : MelonMod
{
    private static int _lastUpdateSecond = -1;

    private IConfigService _configService;
    private IUpdateService _updateService;
    public static bool Reset { get; set; } = true;
    public static bool IsSongDescLoaded { get; private set; }

    public override void OnInitializeMelon()
    {
        var provider = BuildServiceProvider();
        ModServices.Initialize(provider);

        _configService = ModServices.GetRequiredService<IConfigService>();
        _updateService = ModServices.GetRequiredService<IUpdateService>();

        CheckForUpdatesAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
                Melon<MDIPMod>.Logger.Error($"Update check failed: {task.Exception}");
        });
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Melon<MDIPMod>.Logger);
        services.AddSingleton<IConfigService>(provider => new ConfigService(provider.GetRequiredService<MelonLogger.Instance>()));
        services.AddSingleton<IUpdateService>(provider => new UpdateService(provider.GetRequiredService<MelonLogger.Instance>()));
        services.AddSingleton<INoteRecordService>(provider => new NoteRecordService(provider.GetRequiredService<MelonLogger.Instance>()));
        services.AddSingleton<ITextDataService, TextDataService>();
        services.AddSingleton<ITextObjectService>(provider => new TextObjectService(provider.GetRequiredService<ITextDataService>()));
        services.AddSingleton<IStatsSaverService>(provider => new StatsSaverService(provider.GetRequiredService<MelonLogger.Instance>()));
        services.AddSingleton<IFontService, FontService>();
        return services.BuildServiceProvider();
    }

    private async Task CheckForUpdatesAsync()
    {
        var updateInfo = await _updateService.GetUpdateInfoAsync();
        if (updateInfo == null)
        {
            Melon<MDIPMod>.Logger.Error("Failed to fetch update info.");
            return;
        }

        if (!_updateService.IsUpdateAvailable(updateInfo))
        {
            Melon<MDIPMod>.Logger.Msg("Already up to date.");
            return;
        }

        var success = await _updateService.ApplyUpdateAsync(updateInfo);
        if (success)
            Melon<MDIPMod>.Logger.Warning("Auto update successful!");
        else
            Melon<MDIPMod>.Logger.Error("Auto update failed!");
    }

    public override void OnLateInitializeMelon()
    {
        IsSongDescLoaded = RegisteredMelons.Any(mod => mod?.MelonAssembly?.Assembly?.FullName?.TrimStart().StartsWith("SongDesc") ?? false);

        _configService.Init();

        RegisterAndSaveConfig<MainConfigs>(nameof(MainConfigs));
        RegisterAndSaveConfig<AdvancedConfigs>(nameof(AdvancedConfigs));
        RegisterAndSaveConfig<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs));
        RegisterAndSaveConfig<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs));
        RegisterAndSaveConfig<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs));
        RegisterAndSaveConfig<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs));
        RegisterAndSaveConfig<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs));
        RegisterAndSaveConfig<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs));

        _configService.ActivateWatcher();
        return;

        void RegisterAndSaveConfig<T>(string moduleName) where T : ConfigBase, new()
        {
            var fileName = $"{moduleName}.yml";
            _configService.RegisterModule(moduleName, fileName);
            _configService.SaveConfig(moduleName, _configService.GetConfig<T>(moduleName));
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
                BattleUIManager.Reset();
                GameStatsManager.Reset();
                TextObjManager.Reset();
                GameUtils.Reset();
                FontUtils.UnloadFonts();
                break;
        }
    }

    public override void OnFixedUpdate()
    {
        BattleUIManager.CheckAndZoom();
    }

    public override void OnLateUpdate()
    {
        if (!GameStatsManager.IsInGame || _lastUpdateSecond == DateTime.Now.Second)
            return;

        _lastUpdateSecond = DateTime.Now.Second;

        GameStatsManager.UpdateCurrentStats();
        TextObjManager.UpdateAllText();
    }
}