namespace MDIP.Core.Contracts;

public interface IUpdateService
{
    Task<VersionInfo> GetUpdateInfoAsync();
    bool IsUpdateAvailable(VersionInfo updateInfo);
    Task<bool> ApplyUpdateAsync(VersionInfo updateInfo);
}