using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using JetBrains.Annotations;
using MDIP.Application.Services.Logging;
using MDIP.Domain.Updates;
using MDIP.Utils;

namespace MDIP.Application.Services.Updates;

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
            Logger.Error($"Failed to get update info: {ex.Message}");
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
            Logger.Error($"Version comparison failed: {ex.Message}");
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
        var tempPath = Path.Combine(_modFolder, "Info+.temp");
        var newPath = Path.Combine(_modFolder, "Info+.dll");
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
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to clean up old backup or temp file: {ex.Message}");
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

            Logger.Warning($"Hash mismatch for download from {url}");
            SafeDelete(tempPath);
            return false;

        }
        catch (Exception ex)
        {
            Logger.Warning($"Download failed from {url}: {ex.Message}");
            SafeDelete(tempPath);
            return false;
        }
    }

    private bool TryReplaceAssembly(UpdatePaths paths)
    {
        try
        {
            File.Move(_modPath, paths.BackupPath);
            File.Move(paths.TempPath, paths.NewPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"File replacement failed: {ex.Message}");
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

            File.Move(backupPath, _modPath);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to restore backup: {ex.Message}");
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
            Logger.Warning($"Failed to delete {path}: {ex.Message}");
        }
    }

    private readonly struct UpdatePaths(string backupPath, string tempPath, string newPath)
    {
        public string BackupPath { get; } = backupPath;
        public string TempPath { get; } = tempPath;
        public string NewPath { get; } = newPath;
    }

    private static string CalculateFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    [UsedImplicitly] public ILogger<UpdateService> Logger { get; set; }
}