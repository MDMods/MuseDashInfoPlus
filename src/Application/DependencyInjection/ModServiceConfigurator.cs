using MDIP.Application.Services.Assets;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Logging;
using MDIP.Application.Services.Notes;
using MDIP.Application.Services.Stats;
using MDIP.Application.Services.Text;
using MDIP.Application.Services.UI;
using MDIP.Application.Services.Updates;

namespace MDIP.Application.DependencyInjection;

public static class ModServiceConfigurator
{
    public static SimpleServiceProvider Provider { get; private set; }
    public static IServiceScope CurrentScope { get; private set; }

    public static void Build()
    {
        if (Provider != null)
            return;

        var provider = new SimpleServiceProvider();

        // Global singleton services (whole game session)
        provider.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        provider.AddSingletonWithPropertyInjection<IConfigService, ConfigService>();
        provider.AddSingletonWithPropertyInjection<IConfigAccessor, ConfigAccessor>();
        provider.AddSingletonWithPropertyInjection<IFontService, FontService>();
        provider.AddSingletonWithPropertyInjection<IUpdateService, UpdateService>();
        provider.AddSingletonWithPropertyInjection<IStatsSaverService, StatsSaverService>();
        provider.AddSingletonWithPropertyInjection<IPreparationScreenService, PreparationScreenService>();
        provider.AddSingletonWithPropertyInjection<IVictoryScreenService, VictoryScreenService>();

        // Game scoped services (one single game)
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
            Provider.InjectStaticProperties(type);
        }
    }
}