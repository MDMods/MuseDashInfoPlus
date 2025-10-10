﻿using System.Reflection;
using MDIP;
using MDIP.Presentation;

[assembly: MelonInfo(typeof(MDIPMod), ModBuildInfo.Name, ModBuildInfo.Version, ModBuildInfo.Author, ModBuildInfo.RepoLink)]
[assembly: MelonGame("PeroPeroGames", "MuseDash")]
[assembly: MelonColor(255, 255, 79, 113)]
[assembly: MelonAuthorColor(255, 128, 128, 128)]

[assembly: AssemblyDescription(ModBuildInfo.Description)]
[assembly: AssemblyCopyright($"{ModBuildInfo.Author} © 2025; open-source")]