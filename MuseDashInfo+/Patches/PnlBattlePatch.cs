using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

using MDIP.Utils;
using MDIP.Manager;
using MDIP.Modules;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
public class PnlBattleGameStartPatch
{
    private static GameObject TextObjectTemplate;
    private static GameObject ChartInfosObj;
    private static GameObject GameStatsObj;
    private static GameObject ScoreStatsObj;
    private static GameObject NoteStatsObj;

    public static bool IsSpellMode { get; private set; }

    public static void Reset()
    {
        DestroyObjectAndReset(ref TextObjectTemplate);
        DestroyObjectAndReset(ref ChartInfosObj);
        DestroyObjectAndReset(ref GameStatsObj);
        DestroyObjectAndReset(ref ScoreStatsObj);
        DestroyObjectAndReset(ref NoteStatsObj);

        IsSpellMode = false;
    }

    private static void DestroyObjectAndReset(ref GameObject obj)
    {
        if (obj == null) return;
        try { Object.Destroy(obj); } catch { }
        obj = null;
    }

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
                IsSpellMode = true;
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

            // Remove AP icon
            var imgIconAp = curPnlBattleUISub.transform.Find(imgIconApPath)?.gameObject;
            if (imgIconAp != null) imgIconAp.transform.localPosition = new Vector3(9999, 9999, -9999);

            FontUtils.LoadFonts(TextFontType.SnapsTaste);

            // Chart Infos
            if (ConfigManager.DisplayChartName || ConfigManager.DisplayChartDifficulty)
            {
                ChartInfosObj = CreateTextObj(
                    "InfoPlus_ChartInfos",
                    curPnlBattleUISub.transform.Find("Up"),
                    false,
                    TextAnchor.UpperRight,
                    Constants.CHART_INFOS_POS,
                    Constants.CHART_NAME_SIZE
                );
                var chartInfosText = ChartInfosObj.GetComponent<Text>();
                chartInfosText.lineSpacing = 0.8f;
                chartInfosText.text = GameInfosUtils.GetChartInfosString();
                ChartInfosObj.transform.localScale = new Vector3(1, 0.95f, 1);
            }

            // Score Stats 
            if ((ConfigManager.DisplayHighestScore || ConfigManager.DisplayScoreGap) && !IsSpellMode)
            {
                ScoreStatsObj = CreateTextObj(
                    "InfoPlus_ScoreStats",
                    curPnlBattleUISub.transform.Find(imgIconApPath[..imgIconApPath.LastIndexOf('/')]),
                    true,
                    TextAnchor.LowerLeft,
                    new Vector3(Constants.SCORE_STATS_POS.x, Constants.Y_BEHIND_SCORE[stageType], Constants.SCORE_STATS_POS.z),
                    Constants.SCORE_STATS_SIZE,
                    FontStyle.Bold,
                    true
                );
                var scoreStatsRect = ScoreStatsObj.GetComponent<RectTransform>();
                scoreStatsRect.anchorMin = new Vector2(1, 1);
                scoreStatsRect.anchorMax = new Vector2(1, 1);
                scoreStatsRect.pivot = new Vector2(1, 1);
                StatsTextManager.SetScoreStatsInstance(ScoreStatsObj);
            }

            // Game Stats
            if (ConfigManager.DisplayAccuracy || ConfigManager.DisplayMissCounts)
            {
                GameStatsObj = CreateTextObj(
                    "InfoPlus_GameStats",
                    curPnlBattleUISub.transform.Find("Score"),
                    false,
                    TextAnchor.UpperLeft,
                    new Vector3(Constants.X_BEHIND_SCORE_TEXT[stageType], Constants.GAME_STATS_POS.y, Constants.GAME_STATS_POS.z),
                    Constants.GAME_STATS_SIZE,
                    FontStyle.Normal,
                    true
                );
                StatsTextManager.SetGameStatsInstance(GameStatsObj);
            }
            
            // Note Stats
            if (ConfigManager.DisplayNoteCounts && !IsSpellMode)
            {
                NoteStatsObj = CreateTextObj(
                    "InfoPlus_NoteStats",
                    curPnlBattleUISub.transform.Find("Below"),
                    false,
                    TextAnchor.LowerLeft,
                    Constants.NOTE_STATS_POS,
                    Constants.NOTE_STATS_SIZE,
                    FontStyle.Italic,
                    false
                );
                StatsTextManager.SetNoteStatsInstance(NoteStatsObj);
            }
            
            GameStatsUtils.LockHighestScore();
            StatsTextManager.UpdateAllText();
        }
        catch (System.Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }

    private static GameObject CreateTextObj(
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
            if (useCustomFont)
                text.font = FontUtils.GetFont(TextFontType.SnapsTaste);
            text.alignment = alignment;
            text.fontStyle = fontStyle;
            text.fontSize = fontSize;

            obj.transform.localPosition = position;
            return obj;
    }
}