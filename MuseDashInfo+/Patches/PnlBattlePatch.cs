using Il2CppAssets.Scripts.UI.Panels;
using MDIP.Interfaces;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
public class PnlBattleGameStartPatch
{
	private const float zoomSpeed = 2f;
	private const float SIGNIFICANT_Y_CHANGE = 10f;
	private static GameObject TextObjectTemplate;
	private static Transform CurPnlBattle;
	private static Transform ScoreTransform;
	private static float prevY = Constants.SCORE_ZOOM_OUT_Y;
	private static bool isZooming;
	private static bool isZoomingIn;
	private static float targetScale = 3f;
	private static float currentScale = 3f;
	private static float zoomProgress;

	private static float currentY => ScoreTransform?.localPosition.y ?? Constants.SCORE_ZOOM_OUT_Y;

	public static void CheckAndZoom()
	{
		if (CurPnlBattle == null || ScoreTransform == null) return;

		if (Mathf.Abs(currentScale - CurPnlBattle.localScale.y) > 0.01f)
		{
			currentScale = CurPnlBattle.localScale.y;
			targetScale = currentScale;
		}

		var yChange = currentY - prevY;
		if (!isZooming && Mathf.Abs(yChange) > SIGNIFICANT_Y_CHANGE)
		{
			isZoomingIn = yChange < 0;
			isZooming = true;
			zoomProgress = 0f;
			targetScale = isZoomingIn ? 1f : 3f;
		}

		if (isZooming)
		{
			zoomProgress = Mathf.Min(zoomProgress + Time.fixedDeltaTime * zoomSpeed, 1f);

			var easedProgress = isZoomingIn ? EaseOutCubic(zoomProgress) : EaseInCubic(zoomProgress);
			currentScale = Mathf.Lerp(isZoomingIn ? 3f : 1f, targetScale, easedProgress);

			CurPnlBattle.localScale = new(1f, currentScale, 1f);

			if (zoomProgress >= 1f)
			{
				isZooming = false;
				currentScale = targetScale;
			}
		}
		else
			CurPnlBattle.localScale = new(1f, currentScale, 1f);

		prevY = currentY;
	}

	private static float EaseInCubic(float x) => x * x * x;
	private static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);

	public static void Reset()
	{
		TextObjectTemplate = null;
		CurPnlBattle = null;
		ScoreTransform = null;

		prevY = Constants.SCORE_ZOOM_OUT_Y;
		isZooming = false;
		isZoomingIn = false;
		targetScale = 3f;
		currentScale = 3f;
		zoomProgress = 0f;
	}

	private static void Postfix(PnlBattle __instance)
	{
		if (TextObjectTemplate != null) return;

		try
		{
			var pnlBattleOthers = __instance.transform.Find("PnlBattleUI/PnlBattleOthers")?.gameObject;
			var pnlBattleSpell = __instance.transform.Find("PnlBattleUI/PnlBattleSpell").gameObject;
			var pnlBattleWisadel = __instance.transform.Find("PnlBattleUI/PnlBattleWisadel")?.gameObject;
			var curPnlBattle = pnlBattleOthers;
			if (pnlBattleOthers?.active ?? false)
			{
				curPnlBattle = pnlBattleOthers;
				GameUtils.IsOthersMode = true;
			}
			else if (pnlBattleSpell?.active ?? false)
			{
				curPnlBattle = pnlBattleSpell;
				GameUtils.IsSpellMode = true;
			}
			else if (pnlBattleWisadel?.active ?? false)
			{
				curPnlBattle = pnlBattleWisadel;
				GameUtils.IsWisadelMode = true;
			}
			else
				throw new("Unknown battle stage type, everything will not be loaded!");

			// Zoom out all UI
			curPnlBattle.transform.localScale = new(1f, 3f, 1f);
			CurPnlBattle = curPnlBattle.transform;
			ScoreTransform = curPnlBattle.transform.Find("Score");

			TextObjectTemplate = Object.Instantiate(pnlBattleOthers.transform.Find("Score/Djmax/TxtScore_djmax").gameObject);
			Object.Destroy(TextObjectTemplate.transform.Find("ImgIconApDjmax").gameObject);
			Object.Destroy(TextObjectTemplate.GetComponent<ContentSizeFitter>());
			TextObjectTemplate.transform.localPosition = new(9999, 9999, -9999);

			var imgIconApPath = string.Empty;
			var scoreStyleType = ScoreStyleType.Unknown;
			if (curPnlBattle.transform.Find("Score/GC")?.gameObject?.active ?? false)
			{
				scoreStyleType = ScoreStyleType.GC;
				imgIconApPath = "Score/GC/TxtScoreGC/ImgIconApGc";
			}
			else if (curPnlBattle.transform.Find("Score/Djmax")?.gameObject?.active ?? false)
			{
				scoreStyleType = ScoreStyleType.Djmax;
				imgIconApPath = "Score/Djmax/TxtScore_djmax/ImgIconApDjmax";
			}
			else if (curPnlBattle.transform.Find("Score/ArkNight")?.gameObject?.active ?? false)
			{
				scoreStyleType = ScoreStyleType.ArkNight;
				imgIconApPath = "Score/ArkNight/Area/TxtScoreArkNight/ImgIconApArkNight";
			}
			else if (curPnlBattle.transform.Find("Score/Other/ScoreTittle/ImgEnglish")?.gameObject?.active ?? false)
			{
				scoreStyleType = ScoreStyleType.OtherEN;
				imgIconApPath = "Score/Other/TxtScore/ImgIconAp";
			}
			else if (curPnlBattle.transform.Find("Score/Other/ScoreTittle/ImgChinese")?.gameObject?.active ?? false)
			{
				scoreStyleType = ScoreStyleType.OtherCN;
				imgIconApPath = "Score/Other/TxtScore/ImgIconAp";
			}
			else
				throw new("Unknown score style type, everything will not be loaded!");

			// Adjust pause button location
			var imgPauseRect = curPnlBattle.transform.Find("Up/BtnPause/ImgPause").gameObject.GetComponent<RectTransform>();
			imgPauseRect.sizeDelta = new(70, 70);
			imgPauseRect.anchoredPosition = new(0, 0);

			// Hide AP icon
			var imgIconAp = curPnlBattle.transform.Find(imgIconApPath)?.gameObject;
			if (imgIconAp != null) imgIconAp.transform.localPosition = new(9999, 9999, -9999);

			FontUtils.LoadFonts();

			// Text Field Lower Left
			if (Configs.TextFieldLowerLeft.Enabled)
			{
				var obj = CreateTextObj(
					"InfoPlus_TextLowerLeft",
					curPnlBattle.transform.Find("Below"),
					Configs.TextFieldLowerLeft,
					Constants.POS_LOWER_LEFT_TEXT,
					false,
					TextAnchor.LowerLeft,
					FontStyle.Italic
				);
				TextObjManager.TextLowerLeftObj = obj;
			}

			// Text Field Lower Right
			if (Configs.TextFieldLowerRight.Enabled)
			{
				var obj = CreateTextObj(
					"InfoPlus_TextLowerRight",
					curPnlBattle.transform.Find("Below"),
					Configs.TextFieldLowerRight,
					Constants.POS_LOWER_RIGHT_TEXT,
					false,
					TextAnchor.LowerRight,
					FontStyle.Italic
				);
				TextObjManager.TextLowerRightObj = obj;
			}

			// Text Field Score Below
			if (Configs.TextFieldScoreBelow.Enabled)
			{
				var obj = CreateTextObj(
					"InfoPlus_TextScoreBelow",
					curPnlBattle.transform.Find("Score"),
					Configs.TextFieldScoreBelow,
					new(Constants.OFFSET_SCORE_BELOW_TEXT[scoreStyleType].x, Constants.OFFSET_SCORE_BELOW_TEXT[scoreStyleType].y == 0 ? Constants.POS_SCORE_BELOW_TEXT.y : Constants.OFFSET_SCORE_BELOW_TEXT[scoreStyleType].y, Constants.POS_SCORE_BELOW_TEXT.z),
					false,
					TextAnchor.UpperLeft
				);
				TextObjManager.TextScoreBelowObj = obj;
			}

			// Text Field Score Right
			if (Configs.TextFieldScoreRight.Enabled)
			{
				var obj = CreateTextObj(
					"InfoPlus_TextScoreRight",
					curPnlBattle.transform.Find(imgIconApPath[..imgIconApPath.LastIndexOf('/')]),
					Configs.TextFieldScoreRight,
					new(Constants.POS_SCORE_RIGHT_TEXT.x, Constants.OFFSET_SCORE_RIGHT_TEXT[scoreStyleType], Constants.POS_SCORE_RIGHT_TEXT.z),
					true,
					TextAnchor.LowerLeft
				);
				var rect = obj.GetComponent<RectTransform>();
				rect.anchorMin = new(1, 1);
				rect.anchorMax = new(1, 1);
				rect.pivot = new(1, 1);
				TextObjManager.TextScoreRightObj = obj;
			}

			// Text Field Upper Left
			if (Configs.TextFieldUpperLeft.Enabled)
			{
				var obj = CreateTextObj(
					"InfoPlus_TextUpperLeft",
					curPnlBattle.transform.Find("Up"),
					Configs.TextFieldUpperLeft,
					Constants.POS_UPPER_LEFT_TEXT,
					false,
					TextAnchor.UpperLeft,
					FontStyle.Bold
				);
				TextObjManager.TextUpperLeftObj = obj;
			}

			// Text Field Upper Right
			if (Configs.TextFieldUpperRight.Enabled)
			{
				var obj = CreateTextObj(
					"InfoPlus_TextUpperRight",
					curPnlBattle.transform.Find("Up"),
					Configs.TextFieldUpperRight,
					Constants.POS_UPPER_RIGHT_TEXT,
					false,
					TextAnchor.UpperRight
				);
				var text = obj.GetComponent<Text>();
				text.lineSpacing = 0.8f;
				obj.transform.localScale = new(1, 0.95f, 1);
				TextObjManager.TextUpperRightObj = obj;
			}

			GameStatsManager.Init();
			TextDataManager.UpdateConstants();
			TextObjManager.UpdateAllText();
		}
		catch (Exception e)
		{
			Melon<MDIPMod>.Logger.Error(e.ToString());
		}
	}

	private static GameObject CreateTextObj(
		string objectName,
		Transform parent,
		ITextConfig config,
		Vector3 position,
		bool skipRectReset,
		TextAnchor alignment,
		FontStyle fontStyle = FontStyle.Normal)
	{
		var obj = parent.Find(objectName)?.gameObject ??
		          Object.Instantiate(TextObjectTemplate, parent);
		obj.name = objectName;

		Object.Destroy(obj.transform.Find("ImgIconApDjmax")?.gameObject);
		Object.Destroy(obj.GetComponent<ContentSizeFitter>());

		if (!skipRectReset)
		{
			var rectTransform = obj.GetComponent<RectTransform>();
			rectTransform.anchorMin = rectTransform.anchorMax = new(0.5f, 0.5f);
			rectTransform.pivot = new(0.5f, 0.5f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.anchoredPosition3D = Vector3.zero;
			rectTransform.sizeDelta = new(100f, 100f);
			rectTransform.localScale = Vector3.one;
		}

		var text = obj.GetComponent<Text>();
		switch (config.Font)
		{
			case "Snaps Taste":
				text.font = FontUtils.GetFont(FontType.SnapsTaste);
				break;
			case "Lato":
				text.font = FontUtils.GetFont(FontType.LatoRegular);
				break;
			case "Normal":
				text.font = FontUtils.GetFont(FontType.Normal);
				break;
		}

		text.alignment = alignment;
		text.fontStyle = fontStyle;
		text.fontSize = config.FontSize;
		text.color = config.FontColor.ToColor();

		if (config.FontOutlineEnabled)
		{
			var outline = obj.AddComponent<Outline>();
			outline.effectColor = config.FontOutlineColor.ToColor();
			outline.effectDistance = new(config.FontOutlineWidth, config.FontOutlineWidth);
		}

		obj.transform.localPosition = new(
			position.x + config.OffsetX,
			position.y + config.OffsetY,
			position.z);

		return obj;
	}
}