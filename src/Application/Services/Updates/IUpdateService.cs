using MDIP.Domain.Updates;

namespace MDIP.Application.Services.Updates;

public interface IUpdateService
{
    Task<VersionInfo> GetUpdateInfoAsync();
    bool IsUpdateAvailable(VersionInfo updateInfo);
    Task<bool> ApplyUpdateAsync(VersionInfo updateInfo);
}