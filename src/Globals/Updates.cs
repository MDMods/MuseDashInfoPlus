using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using MDIP.Core.Constants;
using MDIP.Core.Domain.Updates;
using MDIP.Presentation;

namespace MDIP.Globals;

// Auto-updater: fetch version info, compare, download + SHA-256 verify, swap the assembly in Mods/.
// I/O- and network-bound, so failures are logged and degrade gracefully rather than throwing.
internal static class Updates
{
    private static readonly HttpClient HttpClient = new();
    private static readonly string ModFolder = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", @"Mods\"));
    private static readonly string ModPath = Assembly.GetExecutingAssembly().Location;

    public static async Task<VersionInfo> GetUpdateInfoAsync()
    {
        try
        {
            var json = await HttpClient.GetStringAsync(Constants.MDIP_UPDATE_INFO_URL);
            return JsonSerializer.Deserialize<VersionInfo>(json);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to get update info.");
            Log.Error(ex);
            return null;
        }
    }

    public static bool IsUpdateAvailable(VersionInfo updateInfo)
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
            Log.Error("Version comparison failed.");
            Log.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// Auto-update: download + SHA-256 verify, move current -> .backup, temp -> new.
    /// Abort if assembly isn't in Mods; restore backup on failure.
    /// </summary>
    public static async Task<bool> ApplyUpdateAsync(VersionInfo updateInfo)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);

        var paths = PreparePaths();

        if (!TryCleanup(paths))
            return false;

        if (!await TryDownloadPackageAsync(updateInfo, paths.TempPath))
            return false;

        return TryReplaceAssembly(paths);
    }

    private static UpdatePaths PreparePaths()
    {
        var backupPath = ModPath + ".backup";
        var tempPath = Path.Combine(ModFolder, ModBuildInfo.Name + ".temp");
        var newPath = Path.Combine(ModFolder, ModBuildInfo.Name + ".dll");
        return new(backupPath, tempPath, newPath);
    }

    private static bool TryCleanup(UpdatePaths paths)
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
            Log.Error("Failed to clean up old files.");
            Log.Error(ex);
            return false;
        }
    }

    private static async Task<bool> TryDownloadPackageAsync(VersionInfo updateInfo, string tempPath)
    {
        foreach (var url in updateInfo.DownloadURL ?? [])
        {
            if (await DownloadAndVerifyAsync(url, tempPath, updateInfo.Hash))
                return true;
        }

        Log.Error("All download sources failed.");
        return false;
    }

    private static async Task<bool> DownloadAndVerifyAsync(string url, string tempPath, string expectedHash)
    {
        try
        {
            var bytes = await HttpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(tempPath, bytes);

            var actualHash = CalculateFileHash(tempPath);
            if (actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                return true;

            Log.Warn($"Hash mismatch for download from {url}");
            SafeDelete(tempPath);
            return false;
        }
        catch (Exception ex)
        {
            Log.Warn($"Download failed from {url}: {ex.Message}");
            SafeDelete(tempPath);
            return false;
        }
    }

    private static bool TryReplaceAssembly(UpdatePaths paths)
    {
        try
        {
            if (!File.Exists(ModPath))
                throw new FileNotFoundException("Current assembly not found", ModPath);

            var currentDir = Path.GetDirectoryName(ModPath);
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

            File.Move(ModPath, paths.BackupPath, true);
            File.Move(paths.TempPath, paths.NewPath, true);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error("File replacement failed.");
            Log.Error(ex);
            SafeDelete(paths.TempPath);
            TryRestoreBackup(paths.BackupPath);
            return false;
        }
    }

    private static void TryRestoreBackup(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
                return;

            if (File.Exists(ModPath))
                File.Delete(ModPath);

            File.Move(backupPath, ModPath, true);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to restore backup.");
            Log.Error(ex);
        }
    }

    private static void SafeDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to delete {path}");
            Log.Warn(ex);
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
}
