namespace MDIP.Core.Domain.Updates;

public class VersionInfo
{
    public string Version { get; init; }
    public string[] DownloadURL { get; init; }
    public string Hash { get; init; }
}