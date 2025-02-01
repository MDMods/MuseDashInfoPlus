namespace MDIP;

/// <summary>
///  MDIP (MuseDashInfoPlus) main mod class
/// </summary>
public class MDIPMod : MelonMod
{
	private static int _lastUpdateSecond = -1;
	public static bool IsSongDescLoaded { get; private set; }

	public override void OnLateInitializeMelon()
	{
		IsSongDescLoaded = RegisteredMelons.Any(mod => mod?.MelonAssembly?.Assembly?.FullName?.TrimStart().StartsWith("SongDesc") ?? false);

		ConfigManager.Init();

		RegisterAndSaveConfig(nameof(MainConfigs), Configs.Main);
		RegisterAndSaveConfig(nameof(AdvancedConfigs), Configs.Advanced);
		RegisterAndSaveConfig(nameof(TextFieldLowerLeftConfigs), Configs.TextFieldLowerLeft);
		RegisterAndSaveConfig(nameof(TextFieldLowerRightConfigs), Configs.TextFieldLowerRight);
		RegisterAndSaveConfig(nameof(TextFieldScoreBelowConfigs), Configs.TextFieldScoreBelow);
		RegisterAndSaveConfig(nameof(TextFieldScoreRightConfigs), Configs.TextFieldScoreRight);
		RegisterAndSaveConfig(nameof(TextFieldUpperLeftConfigs), Configs.TextFieldUpperLeft);
		RegisterAndSaveConfig(nameof(TextFieldUpperRightConfigs), Configs.TextFieldUpperRight);
		return;

		static void RegisterAndSaveConfig<T>(string moduleName, T config) where T : ConfigBase
		{
			var fileName = $"{moduleName}.yml";
			ConfigManager.RegisterModule(moduleName, fileName);
			ConfigManager.SaveConfig(moduleName, config);
		}
	}

	public override void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
		switch (sceneName)
		{
			case "GameMain":
				break;

			case "Welcome":
				ConfigManager.ActivateWatcher();
				break;

			default:
				if (Configs.Advanced.OutputNoteRecordsToDesktop)
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

		if (_lastUpdateSecond == DateTime.Now.Second || !GameStatsManager.IsInGame) return;
		_lastUpdateSecond = DateTime.Now.Second;

		GameStatsManager.CheckMashing();
		TextObjManager.UpdateAllText();
	}
}