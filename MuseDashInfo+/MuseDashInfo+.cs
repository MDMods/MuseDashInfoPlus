using System;
using System.Linq;
using MDIP.Managers;
using MDIP.Modules;
using MDIP.Modules.Configs;
using MDIP.Patches;
using MDIP.Utils;
using MelonLoader;

namespace MDIP;

internal static class ModBuildInfo
{
	public const string NAME = "Info+";
	public const string DESCRIPTION = "Displays additional in-game infos";
	public const string AUTHOR = "KARPED1EM";
	public const string VERSION = "2.1.0";
	public const string REPO_LINK = "https://github.com/MDMods/MuseDashInfoPlus";
}

/// <summary>
///  MDIP (MuseDashInfoPlus) main mod class
/// </summary>
public class MDIPMod : MelonMod
{
	private static int lastUpdateSecond = -1;
	public static MDIPMod Instance { get; private set; }

	public static bool IsSongDescLoaded { get; private set; }

	public override void OnInitializeMelon() => Instance = this;

	public override void OnLateInitializeMelon()
	{
		IsSongDescLoaded = RegisteredMelons.Any(mod => mod.MelonAssembly.Assembly.FullName.TrimStart().StartsWith("SongDesc"));

		ConfigManager.RegisterModule(ConfigName.MainConfigs, "MainConfigs.yml");
		var mainConfigModule = ConfigManager.GetModule(ConfigName.MainConfigs);
		mainConfigModule.RegisterUpdateCallback<MainConfigs>(cfg =>
		{
			Melon<MDIPMod>.Logger.Warning("Main configs updated!");
		});
		ConfigManager.SaveConfig(ConfigName.MainConfigs, Configs.Main);

		ConfigManager.RegisterModule(ConfigName.AdvancedConfigs, "AdvancedConfigs.yml");
		ConfigManager.SaveConfig(ConfigName.AdvancedConfigs, Configs.Advanced);

		ConfigManager.RegisterModule(ConfigName.TextFieldLowerLeftConfigs, "TextFieldLowerLeftConfigs.yml");
		ConfigManager.SaveConfig(ConfigName.TextFieldLowerLeftConfigs, Configs.TextFieldLowerLeft);

		ConfigManager.RegisterModule(ConfigName.TextFieldLowerRightConfigs, "TextFieldLowerRightConfigs.yml");
		ConfigManager.SaveConfig(ConfigName.TextFieldLowerRightConfigs, Configs.TextFieldLowerRight);

		ConfigManager.RegisterModule(ConfigName.TextFieldScoreBelowConfigs, "TextFieldScoreBelowConfigs.yml");
		ConfigManager.SaveConfig(ConfigName.TextFieldScoreBelowConfigs, Configs.TextFieldScoreBelow);

		ConfigManager.RegisterModule(ConfigName.TextFieldScoreRightConfigs, "TextFieldScoreRightConfigs.yml");
		ConfigManager.SaveConfig(ConfigName.TextFieldScoreRightConfigs, Configs.TextFieldScoreRight);

		ConfigManager.RegisterModule(ConfigName.TextFieldUpperLeftConfigs, "TextFieldUpperLeftConfigs.yml");
		ConfigManager.SaveConfig(ConfigName.TextFieldUpperLeftConfigs, Configs.TextFieldUpperLeft);

		ConfigManager.RegisterModule(ConfigName.TextFieldUpperRightConfigs, "TextFieldUpperRightConfigs.yml");
		ConfigManager.SaveConfig(ConfigName.TextFieldUpperRightConfigs, Configs.TextFieldUpperRight);
	}

	public override void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
		switch (sceneName)
		{
			case "GameMain":
				break;

			case "Welcome":
			case "UISystem_PC":
				ConfigManager.ActivateWatcher();
				break;

			default:

				if (Helper.OutputNoteRecordsToDesktop)
					NoteRecordManager.Reset();

				PnlBattleGameStartPatch.Reset();
				GameStatsManager.Reset();
				TextObjManager.Reset();
				GameUtils.Reset();
				FontUtils.UnloadFonts();

				break;
		}
	}

	public override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if (GameStatsManager.IsInGame)
			PnlBattleGameStartPatch.CheckAndZoom();
	}

	public override void OnLateUpdate()
	{
		base.OnLateUpdate();

		if (lastUpdateSecond == DateTime.Now.Second || !GameStatsManager.IsInGame) return;
		lastUpdateSecond = DateTime.Now.Second;

		GameStatsManager.CheckMashing();
		TextObjManager.UpdateAllText();
	}
}