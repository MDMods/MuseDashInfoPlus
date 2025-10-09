using MDIP.Application.Services.Assets;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Logging;
using MDIP.Application.Services.Notes;
using MDIP.Application.Services.Scheduling;
using MDIP.Application.Services.Stats;
using MDIP.Application.Services.Text;
using MDIP.Application.Services.UI;
using MDIP.Application.Services.Updates;

namespace MDIP.Application.DependencyInjection;

public static class ModServiceConfigurator
{
    public static SimpleServiceProvider Provider { get; private set; }
    public static IServiceScope CurrentScope { get; private set; }

    private static readonly HashSet<Type> _staticInjectionTargets = new();

    public static void Build()
    {
        if (Provider != null)
            return;

        var provider = new SimpleServiceProvider();

        provider.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        provider.AddSingletonWithPropertyInjection<IConfigService, ConfigService>();
        provider.AddSingletonWithPropertyInjection<IConfigAccessor, ConfigAccessor>();
        provider.AddSingletonWithPropertyInjection<IFontService, FontService>();
        provider.AddSingletonWithPropertyInjection<IUpdateService, UpdateService>();
        provider.AddSingletonWithPropertyInjection<IStatsSaverService, StatsSaverService>();
        provider.AddSingletonWithPropertyInjection<IPreparationScreenService, PreparationScreenService>();
        provider.AddSingletonWithPropertyInjection<IVictoryScreenService, VictoryScreenService>();
        provider.AddSingletonWithPropertyInjection<IRefreshScheduler, RefreshScheduler>();

        provider.AddScopedWithPropertyInjection<IGameStatsService, GameStatsService>();
        provider.AddScopedWithPropertyInjection<INoteRecordService, NoteRecordService>();
        provider.AddScopedWithPropertyInjection<INoteEventService, NoteEventService>();
        provider.AddScopedWithPropertyInjection<ITextDataService, TextDataService>();
        provider.AddScopedWithPropertyInjection<ITextObjectService, TextObjectService>();
        provider.AddScopedWithPropertyInjection<IBattleUIService, BattleUIService>();

        Provider = provider;
    }

    public static void CreateGameScope()
    {
        DisposeCurrentScope();
        CurrentScope = Provider.CreateScope();
        RefreshStaticInjections();
    }

    public static void DisposeCurrentScope()
    {
        CurrentScope?.Dispose();
        CurrentScope = null;
    }

    public static void Inject(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if (Provider == null)
            throw new InvalidOperationException("Service provider not built.");
        Provider.InjectProperties(instance);
    }

    public static void InjectStatics(params Type[] types)
    {
        if (Provider == null)
            throw new InvalidOperationException("Service provider not built.");

        foreach (var type in types)
        {
            if (type == null)
                continue;

            _staticInjectionTargets.Add(type);
            Provider.InjectStaticProperties(type);
        }
    }

    public static void RefreshStaticInjections()
    {
        if (Provider == null)
            throw new InvalidOperationException("Service provider not built.");

        foreach (var type in _staticInjectionTargets)
            Provider.InjectStaticProperties(type);
    }
}