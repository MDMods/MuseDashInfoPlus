using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

using MuseDashInfoPlus.Utils;
using MuseDashInfoPlus.Manager;
using MuseDashInfoPlus.Modules;

namespace MuseDashInfoPlus.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
public class PnlBattleGameStartPatch
{
    private static GameObject TextObjectTemplate;
    private static GameObject ChartInfosObj;
    private static GameObject GameStatsObj;
    private static GameObject ScoreStatsObj;
    private static GameObject HitStatsObj;

    public static void Reset()
    {
        TextObjectTemplate = null;
        ChartInfosObj = null;
        GameStatsObj = null;
        ScoreStatsObj = null;
        HitStatsObj = null;
    }

    private static void Postfix(PnlBattle __instance)
    {
        if (TextObjectTemplate != null) return;

        try
        {
            var pnlBattleOthers = __instance.transform.Find("PnlBattleUI/PnlBattleOthers").gameObject;
            var curPnlBattleUISub = pnlBattleOthers;
            if (!pnlBattleOthers.active) curPnlBattleUISub = __instance.transform.Find("PnlBattleUI/PnlBattleSpell").gameObject;
            TextObjectTemplate = Object.Instantiate(pnlBattleOthers.transform.Find("Score/Djmax/TxtScore_djmax").gameObject);
            Object.Destroy(TextObjectTemplate.transform.Find("ImgIconApDjmax").gameObject);
            Object.Destroy(TextObjectTemplate.GetComponent<ContentSizeFitter>());
            TextObjectTemplate.transform.localPosition = new Vector3(9999, 9999, -9999);

            GameObject imgIconAp = null;
            var stageType = StageType.Unknown;
            if (curPnlBattleUISub.transform.Find("Score/GC")?.gameObject?.active ?? false)
            {
                stageType = StageType.GC;
                imgIconAp = curPnlBattleUISub.transform.Find("Score/GC/TxtScoreGC/ImgIconApGc").gameObject;
            }
            else if (curPnlBattleUISub.transform.Find("Score/Djmax")?.gameObject?.active ?? false)
            {
                stageType = StageType.Djmax;
                imgIconAp = curPnlBattleUISub.transform.Find("Score/Djmax/TxtScore_djmax/ImgIconApDjmax").gameObject;
            }
            else if (curPnlBattleUISub.transform.Find("Score/Other/ScoreTittle/ImgEnglish")?.gameObject?.active ?? false)
            {
                stageType = StageType.OtherEN;
                imgIconAp = curPnlBattleUISub.transform.Find("Score/Other/TxtScore/ImgIconAp").gameObject;
            }
            else if (curPnlBattleUISub.transform.Find("Score/Other/ScoreTittle/ImgChinese")?.gameObject?.active ?? false)
            {
                stageType = StageType.OtherCN;
                imgIconAp = curPnlBattleUISub.transform.Find("Score/Other/TxtScore/ImgIconAp").gameObject;
            }
            else
            {
                throw new System.Exception("Unknown stage type, everything will not be loaded!");
            }

            // Adjust pause button location
            var imgPauseRect = curPnlBattleUISub.transform.Find("Up/BtnPause/ImgPause").gameObject.GetComponent<RectTransform>();
            imgPauseRect.sizeDelta = new Vector2(70, 70);
            imgPauseRect.anchoredPosition = new Vector2(0, 0);

            FontUtils.LoadFonts(TextFontType.SnapsTaste);

            // Chart Infos
            var chartInfosObj = CreateText(
                "InfoPlus_ChartInfos",
                curPnlBattleUISub.transform.Find("Up"),
                false,
                TextAnchor.UpperRight,
                Constants.CHART_INFOS_POS,
                Constants.CHART_NAME_SIZE
            );
            var chartInfosText = chartInfosObj.GetComponent<Text>();
            chartInfosText.lineSpacing = 0.8f;
            chartInfosText.text = GameInfosUtils.GetChartInfosString();
            chartInfosObj.transform.localScale = new Vector3(1, 0.95f, 1);

            // Score Stats 
            var scoreStatsObj = CreateText(
                "InfoPlus_ScoreStats",
                imgIconAp.transform.parent,
                true,
                TextAnchor.LowerLeft,
                new Vector3(Constants.SCORE_STATS_POS.x, Constants.Y_BEHIND_SCORE[stageType], Constants.SCORE_STATS_POS.z),
                Constants.SCORE_STATS_SIZE,
                FontStyle.Bold,
                true
            );
            Object.Destroy(imgIconAp);
            var scoreStatsRect = scoreStatsObj.GetComponent<RectTransform>();
            scoreStatsRect.anchorMin = new Vector2(1, 1);
            scoreStatsRect.anchorMax = new Vector2(1, 1);
            scoreStatsRect.pivot = new Vector2(1, 1);

            // Game Stats
            var gameStatsObj = CreateText(
                "InfoPlus_GameStats",
                curPnlBattleUISub.transform.Find("Score"),
                false,
                TextAnchor.UpperLeft,
                new Vector3(Constants.X_BEHIND_SCORE_TEXT[stageType], Constants.GAME_STATS_POS.y, Constants.GAME_STATS_POS.z),
                Constants.GAME_STATS_SIZE,
                FontStyle.Normal,
                true
            );

            // Hit Stats
            var hitStatsObj = CreateText(
                "InfoPlus_HitStats",
                curPnlBattleUISub.transform.Find("Below"),
                false,
                TextAnchor.LowerLeft,
                Constants.HIT_STATS_POS,
                Constants.HIT_STATS_SIZE,
                FontStyle.Italic,
                true
            );

            GameStatsUtils.LockHighestScore();
            StatsTextManager.SetGameStatsInstance(gameStatsObj);
            StatsTextManager.SetScoreStatsInstance(scoreStatsObj);
            StatsTextManager.SetHitStatsInstance(hitStatsObj);
            StatsTextManager.UpdateAllText();
        }
        catch (System.Exception e)
        {
            Melon<InfoPlusMod>.Logger.Error(e.ToString());
        }
    }

    private static GameObject CreateText(
        string objectName,
        Transform parent,
        bool skipRectReset,
        TextAnchor alignment,
        Vector3 position,
        int fontSize,
        FontStyle fontStyle = FontStyle.Normal,
        bool useCustomFont = false)
        {
            var obj = parent.Find(objectName)?.gameObject ??
                      Object.Instantiate(TextObjectTemplate, parent);
            obj.name = objectName;

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
            if (useCustomFont)
                text.font = FontUtils.GetFont(TextFontType.SnapsTaste);
            text.alignment = alignment;
            text.fontStyle = fontStyle;
            text.fontSize = fontSize;

            obj.transform.localPosition = position;
            return obj;
    }
}