using MDIP.Application.Services.Global.Assets;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Application.Services.Global.Logging;
using MDIP.Application.Services.Global.Stats;
using MDIP.Application.Services.Global.Updates;
using MDIP.Application.Services.Scoped.Notes;
using MDIP.Application.Services.Scoped.Scheduling;
using MDIP.Application.Services.Scoped.Stats;
using MDIP.Application.Services.Scoped.Text;
using MDIP.Application.Services.Scoped.UI;

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