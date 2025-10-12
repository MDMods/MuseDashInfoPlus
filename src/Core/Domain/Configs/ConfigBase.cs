using MDIP.Presentation;

namespace MDIP.Core.Domain.Configs;

public abstract class ConfigBase
{
    public string Version { get; set; } = ModBuildInfo.Version;
    public DateTime LastModified { get; set; } = DateTime.Now;
}