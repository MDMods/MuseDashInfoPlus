using System.Reflection;
using MDIP.Application.Contracts;
using MDIP.Application.Services.Assets;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Diagnostic;
using MDIP.Application.Services.Notes;
using MDIP.Application.Services.Stats;
using MDIP.Application.Services.Text;
using MDIP.Application.Services.UI;
using MDIP.Infrastructure.Configuration;
using MDIP.Infrastructure.Updates;
using Microsoft.Extensions.DependencyInjection;

namespace MDIP.Application.Bootstrap;

public static class ModServiceConfigurator
{
    public static IServiceProvider Provider { get; private set; }

    public static IServiceProvider Build()
    {
        if (Provider != null)
            return Provider;

        var services = new ServiceCollection();

        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.AddSingleton<IConfigService>(CreateAndConfigure<ConfigService>);
        services.AddSingleton<IConfigAccessor>(CreateAndConfigure<ConfigAccessor>);
        services.AddSingleton<IFontService>(CreateAndConfigure<FontService>);
        services.AddSingleton<ITextDataService>(CreateAndConfigure<TextDataService>);
        services.AddSingleton<ITextObjectService>(CreateAndConfigure<TextObjectService>);
        services.AddSingleton<IStatsSaverService>(CreateAndConfigure<StatsSaverService>);
        services.AddSingleton<INoteRecordService>(CreateAndConfigure<NoteRecordService>);
        services.AddSingleton<IUpdateService>(CreateAndConfigure<UpdateService>);
        services.AddSingleton<IGameStatsService>(CreateAndConfigure<GameStatsService>);
        services.AddSingleton<IBattleUIService>(CreateAndConfigure<BattleUIService>);
        services.AddSingleton<INoteEventService>(CreateAndConfigure<NoteEventService>);
        services.AddSingleton<IPreparationScreenService>(CreateAndConfigure<PreparationScreenService>);
        services.AddSingleton<IVictoryScreenService>(CreateAndConfigure<VictoryScreenService>);

        Provider = services.BuildServiceProvider();
        return Provider;
    }

    public static void Inject(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (Provider == null)
            throw new InvalidOperationException("Service provider not built.");

        InjectProperties(Provider, instance);
    }

    public static void InjectStatics(params Type[] types)
    {
        if (Provider == null)
            throw new InvalidOperationException("Service provider not built.");

        foreach (var type in types)
        {
            foreach (var property in GetInjectableProperties(type, includeStatic: true))
            {
                var service = Provider.GetService(property.PropertyType);
                if (service != null)
                    property.SetValue(null, service);
            }
        }
    }

    private static TService CreateAndConfigure<TService>(IServiceProvider provider) where TService : class
    {
        var instance = ActivatorUtilities.CreateInstance<TService>(provider);
        return InjectProperties(provider, instance, runPostInject: true);
    }

    private static T InjectProperties<T>(IServiceProvider provider, T instance, bool runPostInject = false)
    {
        if (instance == null)
            return default;

        foreach (var property in GetInjectableProperties(instance.GetType(), includeStatic: false))
        {
            var service = provider.GetService(property.PropertyType);
            if (service != null)
                property.SetValue(instance, service);
        }

        if (runPostInject && instance is IPostInjectable postInjectable)
            postInjectable.PostInject();

        return instance;
    }

    private static IEnumerable<PropertyInfo> GetInjectableProperties(Type type, bool includeStatic)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | (includeStatic ? BindingFlags.Static : BindingFlags.Instance);

        return type
            .GetProperties(flags)
            .Where(property => property.SetMethod != null && HasUsedImplicitlyAttribute(property));
    }

    private static bool HasUsedImplicitlyAttribute(PropertyInfo property)
        => property
            .GetCustomAttributes(inherit: true)
            .Any(attribute =>
                attribute.GetType().FullName is "JetBrains.Annotations.UsedImplicitlyAttribute"
                or "Il2CppJetBrains.Annotations.UsedImplicitlyAttribute");
}