using MDIP.Domain.Configs;

namespace MDIP.Application.Services.Configuration;

public interface IConfigService
{
    void Init();
    void ActivateWatcher();
    void RegisterModule(string name, string configFileName);
    T GetConfig<T>(string moduleName) where T : ConfigBase, new();
    void SaveConfig<T>(string moduleName, T config) where T : ConfigBase;
    void RegisterUpdateCallback<T>(string moduleName, Action<T> callback) where T : class;
    string GetConfigFilePath(string fileName);
}