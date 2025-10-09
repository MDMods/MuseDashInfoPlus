using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MDIP.Application.Services.Diagnostic;
using MDIP.Domain.Updates;

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
            var json = await _httpClient.GetStringAsync("https://mdip.leever.cn/version.json");
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
        if (updateInfo == null)
            return false;

        var backupPath = _modPath + ".backup";
        var tempPath = Path.Combine(_modFolder, "Info+.temp");
        var newPath = Path.Combine(_modFolder, "Info+.dll");

        try
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to clean up old backup or temp file: {ex.Message}");
            return false;
        }

        var downloadSuccess = false;
        foreach (var url in updateInfo.DownloadURL ?? [])
        {
            try
            {
                var response = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(tempPath, response);

                var downloadedHash = CalculateFileHash(tempPath);
                if (downloadedHash.Equals(updateInfo.Hash, StringComparison.OrdinalIgnoreCase))
                {
                    downloadSuccess = true;
                    break;
                }

                Logger.Warning($"Hash mismatch for download from {url}");
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Download failed from {url}: {ex.Message}");
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        if (!downloadSuccess)
        {
            Logger.Error("All download sources failed");
            return false;
        }

        try
        {
            File.Move(_modPath, backupPath);
            File.Move(tempPath, newPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"File replacement failed: {ex.Message}");
            if (!File.Exists(backupPath))
                return false;

            try
            {
                if (File.Exists(_modPath))
                    File.Delete(_modPath);
                File.Move(backupPath, _modPath);
            }
            catch
            {
            }

            return false;
        }
    }

    private static string CalculateFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    [UsedImplicitly] public required ILogger<UpdateService> Logger { get; init; }
}