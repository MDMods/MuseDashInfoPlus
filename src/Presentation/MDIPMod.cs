using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Assets;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Application.Services.Global.Logging;
using MDIP.Application.Services.Global.Updates;
using MDIP.Application.Services.Scoped.Scheduling;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Updates;
using MDIP.Presentation.Patches;

namespace MDIP.Presentation;

// ReSharper disable once InconsistentNaming
public class MDIPMod : MelonMod
{
    private readonly HashSet<string> _missingServicesLogged = [];
    public static bool IsSongDescLoaded { get; private set; }

    public override void OnInitializeMelon()
    {
        ModServiceConfigurator.Build();
        ModServiceConfigurator.Inject(this);
        ModServiceConfigurator.InjectStatics(
            typeof(BaseEnemyObjectControllerPatch),
            typeof(BattleEnemyManagerSetPlayResultPatch),
            typeof(GameMissPlayMissCubePatch),
            typeof(MultHitEnemyControllerPatch),
            typeof(PnlBattleGameStartPatch),
            typeof(PnlPreparationPatch),
            typeof(PnlVictorySetDetailInfoPatch),
            typeof(StatisticsManagerPatch)
        );

        _ = Task.Run(CheckForUpdatesAsync);
    }

    public override void OnLateInitializeMelon()
    {
        IsSongDescLoaded = RegisteredMelons.Any(mod => mod?.MelonAssembly?.Assembly?.FullName?.TrimStart().StartsWith("SongDesc") ?? false);

        if (ConfigService == null)
        {
            LogMissingServiceOnce(nameof(ConfigService));
            return;
        }

        ConfigService.Init();
        foreach (var (type, name) in new (Type type, string name)[]
                 {
                     (typeof(MainConfigs), nameof(MainConfigs)),
                     (typeof(AdvancedConfigs), nameof(AdvancedConfigs)),
                     (typeof(TextFieldLowerLeftConfigs), nameof(TextFieldLowerLeftConfigs)),
                     (typeof(TextFieldLowerRightConfigs), nameof(TextFieldLowerRightConfigs)),
                     (typeof(TextFieldScoreBelowConfigs), nameof(TextFieldScoreBelowConfigs)),
                     (typeof(TextFieldScoreRightConfigs), nameof(TextFieldScoreRightConfigs)),
                     (typeof(TextFieldUpperLeftConfigs), nameof(TextFieldUpperLeftConfigs)),
                     (typeof(TextFieldUpperRightConfigs), nameof(TextFieldUpperRightConfigs))
                 })
        {
            RegisterAndSaveConfig(type, name);
        }
        ConfigService.ActivateWatcher();

        LogInfo($"{ModBuildInfo.Name} has loaded correctly!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "Loading":
                ModServiceConfigurator.DisposeCurrentScope();
                FontService?.UnloadFonts();
                break;
        }
    }

    public override void OnFixedUpdate()
    {
        var scheduler = ModServiceConfigurator.Provider?.GetService(typeof(IRefreshScheduler)) as IRefreshScheduler;
        scheduler?.OnFixedUpdateTick();
    }

    public override void OnLateUpdate()
    {
        var scheduler = ModServiceConfigurator.Provider?.GetService(typeof(IRefreshScheduler)) as IRefreshScheduler;
        scheduler?.OnLateUpdateTick();
    }

    private void RegisterAndSaveConfig(Type configType, string moduleName)
    {
        if (ConfigService == null)
        {
            LogMissingServiceOnce(nameof(ConfigService));
            return;
        }

        var fileName = $"{moduleName}.yml";
        ConfigService.RegisterModule(moduleName, fileName);
        var method = typeof(IConfigService).GetMethod(nameof(ConfigService.GetConfig))!
            .MakeGenericMethod(configType);
        var config = method.Invoke(ConfigService, [moduleName]);
        typeof(IConfigService).GetMethod(nameof(ConfigService.SaveConfig))!
            .MakeGenericMethod(configType)
            .Invoke(ConfigService, [moduleName, config]);
    }

    private async Task CheckForUpdatesAsync()
    {
        if (UpdateService == null)
        {
            LogMissingServiceOnce(nameof(UpdateService));
            return;
        }

        try
        {
            var updateInfo = await UpdateService.GetUpdateInfoAsync();
            if (updateInfo == null)
            {
                LogError("Failed to fetch update info.");
                return;
            }
            if (string.IsNullOrWhiteSpace(updateInfo.Hash))
            {
                LogError("Update hash is empty.");
            }

            if (!UpdateService.IsUpdateAvailable(updateInfo))
            {
                LogInfo("Already up to date.");
                return;
            }

            await HandleUpdateAsync(updateInfo);
        }
        catch (Exception ex)
        {
            LogError("Update check failed.");
            LogError(ex.ToString());
        }
    }

    private async Task HandleUpdateAsync(VersionInfo updateInfo)
    {
        if (UpdateService == null)
        {
            LogMissingServiceOnce(nameof(UpdateService));
            return;
        }

        var success = await UpdateService.ApplyUpdateAsync(updateInfo);
        if (success)
            LogWarn("Auto update successful!");
        else
            LogError("Auto update failed!");
    }

    private void LogInfo(string message)
    {
        if (Logger != null) Logger.Info(message);
        else Melon<MDIPMod>.Logger.Msg(message);
    }

    private void LogWarn(string message)
    {
        if (Logger != null) Logger.Warn(message);
        else Melon<MDIPMod>.Logger.Warning(message);
    }

    private void LogError(string message)
    {
        if (Logger != null) Logger.Error(message);
        else Melon<MDIPMod>.Logger.Error(message);
    }

    private void LogMissingServiceOnce(string serviceName)
    {
        if (_missingServicesLogged.Add(serviceName))
            Melon<MDIPMod>.Logger.Warning($"Service '{serviceName}' is not available yet; skipping related operations.");
    }

    [UsedImplicitly] [Inject] public IConfigService ConfigService { get; set; }
    [UsedImplicitly] [Inject] public IUpdateService UpdateService { get; set; }
    [UsedImplicitly] [Inject] public IFontService FontService { get; set; }
    [UsedImplicitly] [Inject] public ILogger<MDIPMod> Logger { get; set; }
}