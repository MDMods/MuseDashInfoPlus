using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Logging;
using MDIP.Core.Constants;
using MDIP.Core.Domain.Updates;
using MDIP.Presentation;

namespace MDIP.Application.Services.Global.Updates;

public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient = new();
    private readonly string _modFolder = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", @"Mods\"));
    private readonly string _modPath = Assembly.GetExecutingAssembly().Location;

    public async Task<VersionInfo> GetUpdateInfoAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync(Constants.MDIP_UPDATE_INFO_URL);
            return JsonSerializer.Deserialize<VersionInfo>(json);
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to get update info.");
            Logger.Error(ex);
            return null;
        }
    }

    public bool IsUpdateAvailable(VersionInfo updateInfo)
    {
        if (updateInfo == null)
            return false;

        try
        {
            var current = Version.Parse(ModBuildInfo.Version);
            var latest = Version.Parse(updateInfo.Version);
            return latest > current;
        }
        catch (Exception ex)
        {
            Logger.Error("Version comparison failed.");
            Logger.Error(ex);
            return false;
        }
    }

    public async Task<bool> ApplyUpdateAsync(VersionInfo updateInfo)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);

        var paths = PreparePaths();

        if (!TryCleanup(paths))
            return false;

        if (!await TryDownloadPackageAsync(updateInfo, paths.TempPath))
            return false;

        return TryReplaceAssembly(paths);
    }

    private UpdatePaths PreparePaths()
    {
        var backupPath = _modPath + ".backup";
        var tempPath = Path.Combine(_modFolder, ModBuildInfo.Name + ".temp");
        var newPath = Path.Combine(_modFolder, ModBuildInfo.Name + ".dll");
        return new(backupPath, tempPath, newPath);
    }

    private bool TryCleanup(UpdatePaths paths)
    {
        try
        {
            if (File.Exists(paths.BackupPath))
                File.Delete(paths.BackupPath);
            if (File.Exists(paths.TempPath))
                File.Delete(paths.TempPath);
            if (File.Exists(paths.NewPath))
                File.Delete(paths.NewPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to clean up old files.");
            Logger.Error(ex);
            return false;
        }
    }

    private async Task<bool> TryDownloadPackageAsync(VersionInfo updateInfo, string tempPath)
    {
        foreach (var url in updateInfo.DownloadURL ?? [])
        {
            if (await DownloadAndVerifyAsync(url, tempPath, updateInfo.Hash))
                return true;
        }

        Logger.Error("All download sources failed");
        return false;
    }

    private async Task<bool> DownloadAndVerifyAsync(string url, string tempPath, string expectedHash)
    {
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(tempPath, bytes);

            var actualHash = CalculateFileHash(tempPath);
            if (actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                return true;

            Logger.Warn($"Hash mismatch for download from {url}");
            SafeDelete(tempPath);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Download failed from {url}: {ex.Message}");
            SafeDelete(tempPath);
            return false;
        }
    }

    private bool TryReplaceAssembly(UpdatePaths paths)
    {
        try
        {
            if (!File.Exists(_modPath))
                throw new FileNotFoundException("Current assembly not found", _modPath);

            var currentDir = Path.GetDirectoryName(_modPath);
            if (string.IsNullOrEmpty(currentDir))
                throw new InvalidOperationException("Failed to resolve current assembly directory");

            var targetDir = Path.GetDirectoryName(paths.NewPath);
            if (string.IsNullOrEmpty(targetDir))
                throw new InvalidOperationException("Failed to resolve target directory");

            if (!string.Equals(currentDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    targetDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Current assembly is not located in the Mods directory");
            }

            File.Move(_modPath, paths.BackupPath, true);
            File.Move(paths.TempPath, paths.NewPath, true);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("File replacement failed.");
            Logger.Error(ex);
            SafeDelete(paths.TempPath);
            TryRestoreBackup(paths.BackupPath);
            return false;
        }
    }

    private void TryRestoreBackup(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
                return;

            if (File.Exists(_modPath))
                File.Delete(_modPath);

            File.Move(backupPath, _modPath, true);
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to restore backup.");
            Logger.Error(ex);
        }
    }

    private void SafeDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to delete {path}.");
            Logger.Warn(ex);
        }
    }

    private static string CalculateFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private readonly struct UpdatePaths(string backupPath, string tempPath, string newPath)
    {
        public string BackupPath { get; } = backupPath;
        public string TempPath { get; } = tempPath;
        public string NewPath { get; } = newPath;
    }

    [UsedImplicitly] [Inject] public ILogger<UpdateService> Logger { get; set; }
}