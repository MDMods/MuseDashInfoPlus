using System;

namespace MDIP.Modules.Configs;

public abstract class ConfigBase
{
	public string Version { get; set; } = ModBuildInfo.Version;
	public DateTime LastModified { get; set; } = DateTime.Now;
}