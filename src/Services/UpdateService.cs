using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace MDIP.Services;

public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient = new();
    private readonly MelonLogger.Instance _logger;
    private readonly string _modFolder;
    private readonly string _modPath;

    public UpdateService(MelonLogger.Instance logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _modPath = Assembly.GetExecutingAssembly().Location;
        _modFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", @"Mods\"));
    }

    public async Task<VersionInfo> GetUpdateInfoAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync("https://mdip.leever.cn/version.json");
            return JsonSerializer.Deserialize<VersionInfo>(json);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get update info: {ex.Message}");
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
            _logger.Error($"Version comparison failed: {ex.Message}");
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
            _logger.Error($"Failed to clean up old backup or temp file: {ex.Message}");
            return false;
        }

        var downloadSuccess = false;
        foreach (var url in updateInfo.DownloadURL ?? Array.Empty<string>())
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

                _logger.Warning($"Hash mismatch for download from {url}");
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Download failed from {url}: {ex.Message}");
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        if (!downloadSuccess)
        {
            _logger.Error("All download sources failed");
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
            _logger.Error($"File replacement failed: {ex.Message}");
            if (File.Exists(backupPath))
            {
                try
                {
                    if (File.Exists(_modPath))
                        File.Delete(_modPath);
                    File.Move(backupPath, _modPath);
                }
                catch
                {
                }
            }
            return false;
        }
    }

    private static string CalculateFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}