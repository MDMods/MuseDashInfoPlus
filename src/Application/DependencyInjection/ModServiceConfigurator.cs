using System;
using Microsoft.Extensions.DependencyInjection;
using MDIP.Application.Services.Assets;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Diagnostic;
using MDIP.Application.Services.Notes;
using MDIP.Application.Services.Stats;
using MDIP.Application.Services.Text;
using MDIP.Application.Services.UI;
using MDIP.Application.Services.Updates;

namespace MDIP.Application.DependencyInjection;

public static class ModServiceConfigurator
{
    public static IServiceProvider Provider { get; private set; }

    public static void Build()
    {
        if (Provider != null)
            return;

        var services = new ServiceCollection();

        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.AddSingletonWithPropertyInjection<IConfigService, ConfigService>();
        services.AddSingletonWithPropertyInjection<IConfigAccessor, ConfigAccessor>();
        services.AddSingletonWithPropertyInjection<IFontService, FontService>();
        services.AddSingletonWithPropertyInjection<ITextDataService, TextDataService>();
        services.AddSingletonWithPropertyInjection<ITextObjectService, TextObjectService>();
        services.AddSingletonWithPropertyInjection<IStatsSaverService, StatsSaverService>();
        services.AddSingletonWithPropertyInjection<INoteRecordService, NoteRecordService>();
        services.AddSingletonWithPropertyInjection<IUpdateService, UpdateService>();
        services.AddSingletonWithPropertyInjection<IGameStatsService, GameStatsService>();
        services.AddSingletonWithPropertyInjection<IBattleUIService, BattleUIService>();
        services.AddSingletonWithPropertyInjection<INoteEventService, NoteEventService>();
        services.AddSingletonWithPropertyInjection<IPreparationScreenService, PreparationScreenService>();
        services.AddSingletonWithPropertyInjection<IVictoryScreenService, VictoryScreenService>();

        Provider = services.BuildServiceProvider();
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