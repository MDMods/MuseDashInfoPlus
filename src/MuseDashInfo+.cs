using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Assets;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Diagnostic;
using MDIP.Application.Services.Notes;
using MDIP.Application.Services.Stats;
using MDIP.Application.Services.Text;
using MDIP.Application.Services.UI;
using MDIP.Application.Services.Updates;
using MDIP.Domain.Configs;
using MDIP.Domain.Updates;
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

        _ = CheckForUpdatesAsync(); // Fire and forget
    }

    public override void OnLateInitializeMelon()
    {
        IsSongDescLoaded = RegisteredMelons.Any(mod => mod?.MelonAssembly?.Assembly?.FullName?.TrimStart().StartsWith("SongDesc") ?? false);

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
        try
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

            await HandleUpdateAsync(updateInfo);
        }
        catch (Exception ex)
        {
            Logger.Error($"Update check failed: {ex}");
        }
    }

    private async Task HandleUpdateAsync(VersionInfo updateInfo)
    {
        var success = await UpdateService.ApplyUpdateAsync(updateInfo);
        if (success)
            Logger.Warning("Auto update successful!");
        else
            Logger.Error("Auto update failed!");
    }

    [UsedImplicitly] public IConfigService ConfigService { get; set; }
    [UsedImplicitly] public IUpdateService UpdateService { get; set; }
    [UsedImplicitly] public ITextObjectService TextObjectService { get; set; }
    [UsedImplicitly] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] public IBattleUIService BattleUIService { get; set; }
    [UsedImplicitly] public INoteRecordService NoteRecordService { get; set; }
    [UsedImplicitly] public IFontService FontService { get; set; }
    [UsedImplicitly] public ILogger<MDIPMod> Logger { get; set; }
}