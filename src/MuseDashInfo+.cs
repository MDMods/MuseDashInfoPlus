using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Assets;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Logging;
using MDIP.Application.Services.Stats;
using MDIP.Application.Services.Text;
using MDIP.Application.Services.UI;
using MDIP.Application.Services.Updates;
using MDIP.Domain.Configs;
using MDIP.Domain.Updates;
using MDIP.Patches;

namespace MDIP;

// ReSharper disable once InconsistentNaming
public class MDIPMod : MelonMod
{
    private static int _lastUpdateSecond = -1;
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

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "UISystem_PC":
                ModServiceConfigurator.DisposeCurrentScope();
                break;
            case "Loading":
                ModServiceConfigurator.DisposeCurrentScope();
                FontService?.UnloadFonts();
                break;
        }
    }

    public override void OnFixedUpdate()
    {
        if (BattleUIService == null)
        {
            LogMissingServiceOnce(nameof(BattleUIService));
            return;
        }

        BattleUIService.CheckAndZoom();
    }

    public override void OnLateUpdate()
    {
        if (GameStatsService == null)
        {
            LogMissingServiceOnce(nameof(GameStatsService));
            return;
        }

        if (!GameStatsService.IsInGame || _lastUpdateSecond == DateTime.Now.Second)
            return;

        if (TextObjectService == null)
        {
            LogMissingServiceOnce(nameof(TextObjectService));
            return;
        }

        _lastUpdateSecond = DateTime.Now.Second;
        GameStatsService.UpdateCurrentStats();
        TextObjectService.UpdateAllText();
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
            LogError($"Update check failed: {ex}");
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
            LogWarning("Auto update successful!");
        else
            LogError("Auto update failed!");
    }

    private void LogInfo(string message)
    {
        if (Logger != null) Logger.Info(message);
        else Melon<MDIPMod>.Logger.Msg($"[Info+] {message}");
    }

    private void LogWarning(string message)
    {
        if (Logger != null) Logger.Warn(message);
        else Melon<MDIPMod>.Logger.Warning($"[Info+] {message}");
    }

    private void LogError(string message)
    {
        if (Logger != null) Logger.Error(message);
        else Melon<MDIPMod>.Logger.Error($"[Info+] {message}");
    }

    private void LogMissingServiceOnce(string serviceName)
    {
        if (_missingServicesLogged.Add(serviceName))
            Melon<MDIPMod>.Logger.Warning($"[Info+] Service '{serviceName}' is not available yet; skipping related operations.");
    }

    [UsedImplicitly] [Inject] public IConfigService ConfigService { get; set; }
    [UsedImplicitly] [Inject] public IUpdateService UpdateService { get; set; }
    [UsedImplicitly] [Inject] public ITextObjectService TextObjectService { get; set; }
    [UsedImplicitly] [Inject] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] [Inject] public IBattleUIService BattleUIService { get; set; }
    [UsedImplicitly] [Inject] public IFontService FontService { get; set; }
    [UsedImplicitly] [Inject] public ILogger<MDIPMod> Logger { get; set; }
}