using MDIP.Core.Domain.Updates;

namespace MDIP.Application.Services.Global.Updates;

public interface IUpdateService
{
    Task<VersionInfo> GetUpdateInfoAsync();
    bool IsUpdateAvailable(VersionInfo updateInfo);
    Task<bool> ApplyUpdateAsync(VersionInfo updateInfo);
}