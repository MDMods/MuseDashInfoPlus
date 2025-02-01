using System;

namespace MDIP.Modules.Configs;

public abstract class ConfigBase
{
	public string Version { get; set; } = ModBuildInfo.VERSION;
	public DateTime LastModified { get; set; } = DateTime.Now;
}