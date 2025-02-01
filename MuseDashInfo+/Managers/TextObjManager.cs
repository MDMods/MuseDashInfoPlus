using UnityEngine.UI;

namespace MDIP.Managers;

public static class TextObjManager
{
	private static long lastUpdateTick;
	public static GameObject TextLowerLeftObj { get; set; }
	public static GameObject TextLowerRightObj { get; set; }
	public static GameObject TextScoreBelowObj { get; set; }
	public static GameObject TextScoreRightObj { get; set; }
	public static GameObject TextUpperLeftObj { get; set; }
	public static GameObject TextUpperRightObj { get; set; }

	public static void UpdateAllText()
	{
		var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		if (now - lastUpdateTick < Configs.Advanced.DataRefreshIntervalLimit) return;
		lastUpdateTick = now;

		GameStatsManager.UpdateCurrentStats();
		TextDataManager.UpdateValues();

		if (TextLowerLeftObj != null && Configs.TextFieldLowerLeft.Enabled)
			TextLowerLeftObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldLowerLeft.Text.UnEscapeReturn()));

		if (TextLowerRightObj != null && Configs.TextFieldLowerRight.Enabled)
			TextLowerRightObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldLowerRight.Text.UnEscapeReturn()));

		if (TextScoreBelowObj != null && Configs.TextFieldScoreBelow.Enabled)
			TextScoreBelowObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldScoreBelow.Text.UnEscapeReturn()));

		if (TextScoreRightObj != null && Configs.TextFieldScoreRight.Enabled)
			TextScoreRightObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldScoreRight.Text.UnEscapeReturn()));

		if (TextUpperLeftObj != null && Configs.TextFieldUpperLeft.Enabled)
			TextUpperLeftObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldUpperLeft.Text.UnEscapeReturn()));

		if (TextUpperRightObj != null && Configs.TextFieldUpperRight.Enabled)
			TextUpperRightObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldUpperRight.Text.UnEscapeReturn()));
	}

	private static void SetText(this GameObject obj, string text)
	{
		if (obj == null) return;
		var textComponent = obj.GetComponent<Text>();
		if (textComponent == null) return;
		textComponent.text = text;
	}

	public static void Reset()
	{
		GameObject.Destroy(TextLowerLeftObj);
		TextLowerLeftObj = null;
		GameObject.Destroy(TextLowerRightObj);
		TextLowerRightObj = null;
		GameObject.Destroy(TextScoreBelowObj);
		TextScoreBelowObj = null;
		GameObject.Destroy(TextScoreRightObj);
		TextScoreRightObj = null;
		GameObject.Destroy(TextUpperLeftObj);
		TextUpperLeftObj = null;
		GameObject.Destroy(TextUpperRightObj);
		TextUpperRightObj = null;
	}
}