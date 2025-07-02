using System.Security.Cryptography;
using System.Text.Json;

namespace MDIP.Managers;

public class UpdateManager
{
    private static readonly HttpClient _httpClient = new();
    private readonly string _modPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Mods", "Info+.dll"));

    public async static Task<VersionInfo> GetUpdateInfo()
    {
        try
        {
            var json = await _httpClient.GetStringAsync("https://mdip.leever.cn/version.json");
            return JsonSerializer.Deserialize<VersionInfo>(json);
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.Error($"Failed to get update info: {ex.Message}");
            return null;
        }
    }

    public bool CheckUpdate(VersionInfo updateInfo)
    {
        if (updateInfo == null) return false;

        try
        {
            var current = Version.Parse(ModBuildInfo.Version);
            var latest = Version.Parse(updateInfo.Version);
            return latest > current;
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.Error($"Version comparison failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Update(VersionInfo updateInfo)
    {
        if (updateInfo == null) return false;

        var backupPath = _modPath + ".backup";
        var tempPath = _modPath + ".temp";

        // Clean up old backup if exists
        try
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.Error($"Failed to clean up old backup: {ex.Message}");
            return false;
        }

        // Try downloading from each URL
        var downloadSuccess = false;
        foreach (var url in updateInfo.DownloadURL)
        {
            try
            {
                var response = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(tempPath, response);

                // Verify file hash
                var downloadedHash = CalculateFileHash(tempPath);
                if (downloadedHash.Equals(updateInfo.Hash, StringComparison.OrdinalIgnoreCase))
                {
                    downloadSuccess = true;
                    break;
                }
                MelonLogger.Warning($"Hash mismatch for download from {url}");
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Download failed from {url}: {ex.Message}");
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        if (!downloadSuccess)
        {
            Melon<MDIPMod>.Logger.Error("All download sources failed");
            return false;
        }

        try
        {
            // Backup current file
            File.Move(_modPath, backupPath);
            // Move new file to correct location
            File.Move(tempPath, _modPath);
            return true;
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.Error($"File replacement failed: {ex.Message}");
            // Try to restore
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
                // ignored
            }
            return false;
        }
    }

    private static string CalculateFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}