using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

using MDIP.Interfaces;
using MDIP.Managers;
using MDIP.Modules;
using MDIP.Utils;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
public class PnlBattleGameStartPatch
{
    private static GameObject TextObjectTemplate;

    private static void Postfix(PnlBattle __instance)
    {
        if (TextObjectTemplate != null) return;

        try
        {
            var pnlBattleOthers = __instance.transform.Find("PnlBattleUI/PnlBattleOthers")?.gameObject;
            var curPnlBattleUISub = pnlBattleOthers;
            if (!pnlBattleOthers?.activeSelf ?? false)
            {
                curPnlBattleUISub = __instance.transform.Find("PnlBattleUI/PnlBattleSpell").gameObject;
                GameUtils.IsSpellMode = true;
            }
            TextObjectTemplate = Object.Instantiate(pnlBattleOthers.transform.Find("Score/Djmax/TxtScore_djmax").gameObject);
            Object.Destroy(TextObjectTemplate.transform.Find("ImgIconApDjmax").gameObject);
            Object.Destroy(TextObjectTemplate.GetComponent<ContentSizeFitter>());
            TextObjectTemplate.transform.localPosition = new Vector3(9999, 9999, -9999);

            string imgIconApPath = string.Empty;
            var stageType = StageType.Unknown;
            if (curPnlBattleUISub.transform.Find("Score/GC")?.gameObject?.active ?? false)
            {
                stageType = StageType.GC;
                imgIconApPath = "Score/GC/TxtScoreGC/ImgIconApGc";
            }
            else if (curPnlBattleUISub.transform.Find("Score/Djmax")?.gameObject?.active ?? false)
            {
                stageType = StageType.Djmax;
                imgIconApPath = "Score/Djmax/TxtScore_djmax/ImgIconApDjmax";
            }
            else if (curPnlBattleUISub.transform.Find("Score/Other/ScoreTittle/ImgEnglish")?.gameObject?.active ?? false)
            {
                stageType = StageType.OtherEN;
                imgIconApPath = "Score/Other/TxtScore/ImgIconAp";
            }
            else if (curPnlBattleUISub.transform.Find("Score/Other/ScoreTittle/ImgChinese")?.gameObject?.active ?? false)
            {
                stageType = StageType.OtherCN;
                imgIconApPath = "Score/Other/TxtScore/ImgIconAp";
            }
            else
            {
                throw new System.Exception("Unknown stage type, everything will not be loaded!");
            }

            // Adjust pause button location
            var imgPauseRect = curPnlBattleUISub.transform.Find("Up/BtnPause/ImgPause").gameObject.GetComponent<RectTransform>();
            imgPauseRect.sizeDelta = new Vector2(70, 70);
            imgPauseRect.anchoredPosition = new Vector2(0, 0);

            // Hide AP icon
            var imgIconAp = curPnlBattleUISub.transform.Find(imgIconApPath)?.gameObject;
            if (imgIconAp != null) imgIconAp.transform.localPosition = new Vector3(9999, 9999, -9999);

            FontUtils.LoadFonts();

            // Text Field Lower Left
            if (Configs.TextFieldLowerLeft.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextLowerLeft",
                    curPnlBattleUISub.transform.Find("Below"),
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
                    curPnlBattleUISub.transform.Find("Below"),
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
                    curPnlBattleUISub.transform.Find("Score"),
                    Configs.TextFieldScoreBelow,
                    new Vector3(Constants.X_OFFSET_SCORE_BELOW_TEXT[stageType], Constants.POS_SCORE_BELOW_TEXT.y, Constants.POS_SCORE_BELOW_TEXT.z),
                    false,
                    TextAnchor.UpperLeft,
                    FontStyle.Normal
                );
                TextObjManager.TextScoreBelowObj = obj;
            }

            // Text Field Score Right
            if (Configs.TextFieldScoreRight.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextScoreRight",
                    curPnlBattleUISub.transform.Find(imgIconApPath[..imgIconApPath.LastIndexOf('/')]),
                    Configs.TextFieldScoreRight,
                    new Vector3(Constants.POS_SCORE_RIGHT_TEXT.x, Constants.Y_OFFSET_SCORE_RIGHT_TEXT[stageType], Constants.POS_SCORE_RIGHT_TEXT.z),
                    true,
                    TextAnchor.LowerLeft,
                    FontStyle.Bold
                );
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                TextObjManager.TextScoreRightObj = obj;
            }

            // Text Field Upper Left
            if (Configs.TextFieldUpperLeft.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextUpperLeft",
                    curPnlBattleUISub.transform.Find("Up"),
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
                    curPnlBattleUISub.transform.Find("Up"),
                    Configs.TextFieldUpperRight,
                    Constants.POS_UPPER_RIGHT_TEXT,
                    false,
                    TextAnchor.UpperRight,
                    FontStyle.Normal
                );
                var text = obj.GetComponent<Text>();
                text.lineSpacing = 0.8f;
                obj.transform.localScale = new Vector3(1, 0.95f, 1);
                TextObjManager.TextUpperRightObj = obj;
            }

            GameStatsManager.Init();
            TextDataManager.UpdateConstants();
            TextObjManager.UpdateAllText();
        }
        catch (System.Exception e)
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

            Object.Destroy(obj.transform.Find("ImgIconApDjmax").gameObject);
            Object.Destroy(obj.GetComponent<ContentSizeFitter>());

            if (!skipRectReset)
            {
                var rectTransform = obj.GetComponent<RectTransform>();
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.anchoredPosition3D = Vector3.zero;
                rectTransform.sizeDelta = new Vector2(100f, 100f);
                rectTransform.localScale = Vector3.one;
            }

            var text = obj.GetComponent<Text>();
            switch (config.Font)
            {
                case "Snaps Taste":
                    text.font = FontUtils.GetFont(FontType.SnapsTaste);
                    break;
            }
            text.alignment = alignment;
            text.fontStyle = fontStyle;
            text.fontSize = config.FontSize;
            text.color = config.FontColor.ToColor();
            
            obj.transform.localPosition = new(
                position.x + config.OffsetX,
                position.y + config.OffsetY,
                position.z);

            return obj;
        }
}