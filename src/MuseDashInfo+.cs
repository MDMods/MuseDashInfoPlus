using JetBrains.Annotations;
using MDIP.Application.Bootstrap;
using MDIP.Application.Contracts;
using MDIP.Domain.Configs;
using MDIP.Patches;
using MDIP.Utils;

namespace MDIP;

public class MDIPMod : MelonMod
{
    private static int _lastUpdateSecond = -1;
    public static bool Reset { get; set; } = true;
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

        CheckForUpdatesAsync().ContinueWith(task =>
        {
            if (task is { IsFaulted: true, Exception: not null })
                Logger.Error($"Update check failed: {task.Exception}");
        });
    }

    public override void OnLateInitializeMelon()
    {
        IsSongDescLoaded = RegisteredMelons.Any(mod => mod?.MelonAssembly?.Assembly?.FullName?.TrimStart().StartsWith("SongDesc") ?? false);

        ConfigService.Init();
        RegisterAndSaveConfig<MainConfigs>(nameof(MainConfigs));
        RegisterAndSaveConfig<AdvancedConfigs>(nameof(AdvancedConfigs));
        RegisterAndSaveConfig<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs));
        RegisterAndSaveConfig<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs));
        RegisterAndSaveConfig<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs));
        RegisterAndSaveConfig<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs));
        RegisterAndSaveConfig<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs));
        RegisterAndSaveConfig<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs));
        ConfigService.ActivateWatcher();
        return;

        void RegisterAndSaveConfig<T>(string moduleName) where T : ConfigBase, new()
        {
            var fileName = $"{moduleName}.yml";
            ConfigService.RegisterModule(moduleName, fileName);
            ConfigService.SaveConfig(moduleName, ConfigService.GetConfig<T>(moduleName));
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        switch (sceneName)
        {
            case "UISystem_PC":
                GameStatsService.Reset(true);
                break;
            case "Loading" when !Reset:
                Reset = true;
                NoteRecordService.Reset();
                BattleUIService.Reset();
                GameStatsService.Reset();
                TextObjectService.Reset();
                GameUtils.Reset();
                FontService.UnloadFonts();
                break;
        }
    }

    public override void OnFixedUpdate()
    {
        BattleUIService.CheckAndZoom();
    }

    public override void OnLateUpdate()
    {
        if (!GameStatsService.IsInGame || _lastUpdateSecond == DateTime.Now.Second)
            return;
        _lastUpdateSecond = DateTime.Now.Second;
        GameStatsService.UpdateCurrentStats();
        TextObjectService.UpdateAllText();
    }

    private async Task CheckForUpdatesAsync()
    {
        var updateInfo = await UpdateService.GetUpdateInfoAsync();
        if (updateInfo == null)
        {
            Logger.Error("Failed to fetch update info.");
            return;
        }

        if (!UpdateService.IsUpdateAvailable(updateInfo))
        {
            Logger.Info("Already up to date.");
            return;
        }

        var success = await UpdateService.ApplyUpdateAsync(updateInfo);
        if (success)
            Logger.Warning("Auto update successful!");
        else
            Logger.Error("Auto update failed!");
    }

    #region Injections

    [UsedImplicitly] public required IConfigService ConfigService { get; init; }
    [UsedImplicitly] public required IUpdateService UpdateService { get; init; }
    [UsedImplicitly] public required IConfigAccessor ConfigAccessor { get; init; }
    [UsedImplicitly] public required ITextObjectService TextObjectService { get; init; }
    [UsedImplicitly] public required ITextDataService TextDataService { get; init; }
    [UsedImplicitly] public required IGameStatsService GameStatsService { get; init; }
    [UsedImplicitly] public required IBattleUIService BattleUIService { get; init; }
    [UsedImplicitly] public required INoteRecordService NoteRecordService { get; init; }
    [UsedImplicitly] public required IStatsSaverService StatsSaverService { get; init; }
    [UsedImplicitly] public required IFontService FontService { get; init; }
    [UsedImplicitly] public required ILogger<MDIPMod> Logger { get; init; }

    #endregion
}