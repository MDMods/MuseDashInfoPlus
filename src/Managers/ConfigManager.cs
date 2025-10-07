namespace MDIP.Managers;

public static class ConfigManager
{
    private static IConfigService Service => ModServices.GetRequiredService<IConfigService>();

    public static void Init() => Service.Init();
    public static void ActivateWatcher() => Service.ActivateWatcher();
    public static void RegisterModule(string name, string configFileName) => Service.RegisterModule(name, configFileName);
    public static T GetConfig<T>(string moduleName) where T : ConfigBase, new() => Service.GetConfig<T>(moduleName);
    public static void SaveConfig<T>(string moduleName, T config) where T : ConfigBase => Service.SaveConfig(moduleName, config);
    public static void RegisterUpdateCallback<T>(string moduleName, Action<T> callback) where T : class => Service.RegisterUpdateCallback(moduleName, callback);
    public static string GetConfigPath(string fileName) => Service.GetConfigFilePath(fileName);
}