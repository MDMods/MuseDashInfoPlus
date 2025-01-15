using System.Reflection;
using MelonLoader;

using MDIP;

[assembly: MelonInfo(typeof(MDIP.MDIPMod), ModBuildInfo.NAME, ModBuildInfo.VERSION, ModBuildInfo.AUTHOR, ModBuildInfo.REPO_LINK)]
[assembly: MelonGame("PeroPeroGames", "MuseDash")]
[assembly: MelonColor(255, 255, 79, 113)]
[assembly: MelonAuthorColor(255, 128, 128, 128)]

[assembly: AssemblyDescription(ModBuildInfo.DESCRIPTION)]
[assembly: AssemblyCopyright($"{ModBuildInfo.AUTHOR} © 2024; open-source")]