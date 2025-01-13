﻿using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.UI.Panels;
using MelonLoader;
using UnityEngine;

using MuseDashInfoPlus.Utils;
using MuseDashInfoPlus.Manager;

namespace MuseDashInfoPlus.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
public class PnlBattleGameStartPatch
{
    private static void Postfix(PnlBattle __instance)
    {
        try
        {
            string musicName = GlobalDataBase.dbBattleStage.selectedMusicName;
            int musicDiff = GlobalDataBase.dbBattleStage.selectedDifficulty;
            string musicLevel = GlobalDataBase.dbBattleStage.selectedMusicInfo.GetMusicLevelStringByDiff(musicDiff);
            string musicDiffStr = (musicDiff switch
            {
                1 => "Easy",
                2 => "Hard",
                3 => "Master",
                4 => "Hidden",
                5 => "Special",
                _ => "Unknown"
            }).ToUpper();

            var pnlBattleOthers = __instance.transform.Find("PnlBattleUI/PnlBattleOthers").gameObject;
            var curPnlBattleUISub = pnlBattleOthers;
            if (!pnlBattleOthers.active) curPnlBattleUISub = __instance.transform.Find("PnlBattleUI/PnlBattleSpell").gameObject;
            var imgPauseRect = curPnlBattleUISub.transform.Find("Up/BtnPause/ImgPause").gameObject.GetComponent<RectTransform>();
            imgPauseRect.sizeDelta = new Vector2(70, 70);
            imgPauseRect.anchoredPosition = new Vector2(0, 0);

            var txtScoreSample = pnlBattleOthers.transform.Find("Score/Djmax/TxtScore_djmax").gameObject;
            Object.Destroy(txtScoreSample.transform.FindChild("ImgIconApDjmax").gameObject);
            var txtScoreSampleText = txtScoreSample.GetComponent<UnityEngine.UI.Text>();

            var plusInfos = pnlBattleOthers.transform.Find("KPD_PlusInfos")?.gameObject ?? Object.Instantiate(txtScoreSample, curPnlBattleUISub.transform.Find("Up"));
            var plusInfosText = plusInfos.GetComponent<UnityEngine.UI.Text>();
            plusInfos.name = "KPD_PlusInfos";
            plusInfosText.alignment = TextAnchor.UpperRight;
            plusInfosText.lineSpacing = 0.8f;
            plusInfosText.fontSize = Constants.SONG_NAME_SIZE;
            string songInfoText = string.Empty;
            if (ConfigManager.DisplaySongName) songInfoText = $"<b>{musicName}</b>\n";
            if (ConfigManager.DisplaySongDifficulty) songInfoText += $"<size={Constants.SONG_DIFFICULTY_SIZE}>{ConfigManager.FinalSongDifficultyTextFormat.Replace("{diff}", musicDiffStr).Replace("{level}", musicLevel)}</size>";
            plusInfosText.text = songInfoText;
            var plusInfosRect = plusInfos.GetComponent<RectTransform>();
            plusInfosRect.anchorMin = new Vector2(1, 1);
            plusInfosRect.anchorMax = new Vector2(1, 1);
            plusInfosRect.pivot = new Vector2(1, 1);
            plusInfos.transform.localPosition = Constants.SONG_INFOS_POS;
            plusInfos.transform.localScale = new Vector3(1, 0.95f, 1);

            var plusCounts = pnlBattleOthers.transform.Find("KPD_PlusCounts")?.gameObject ?? Object.Instantiate(txtScoreSample, curPnlBattleUISub.transform.Find("Score"));
            var plusCountsText = plusCounts.GetComponent<UnityEngine.UI.Text>();
            plusCounts.name = "KPD_PlusCounts";
            UITextUtils.LoadFonts(TextFontType.SnapsTaste);
            plusCountsText.font = UITextUtils.GetFont(TextFontType.SnapsTaste);
            plusCountsText.alignment = TextAnchor.UpperLeft;
            plusCountsText.fontStyle = FontStyle.Normal;
            plusCountsText.lineSpacing = 0.8f;
            plusCountsText.fontSize = Constants.COUNTS_PRIMARY_SIZE;

            var stageType = StageType.OtherCN;
            if (curPnlBattleUISub.transform.Find("Score/GC")?.gameObject?.active ?? false)
                stageType = StageType.GC;
            else if (curPnlBattleUISub.transform.Find("Score/Djmax")?.gameObject?.active ?? false)
                stageType = StageType.Djmax;
            else if (curPnlBattleUISub.transform.Find("Score/Other/ScoreTittle/ImgEnglish")?.gameObject?.active ?? false)
                stageType = StageType.OtherEN;
            else if (curPnlBattleUISub.transform.Find("Score/Other/ScoreTittle/ImgChinese")?.gameObject?.active ?? false)
                stageType = StageType.OtherCN;
            else
                stageType = StageType.Unknown;

            float x = 0;
            x = stageType switch
            {
                StageType.OtherCN => 110,
                StageType.OtherEN => 220,
                StageType.GC => 278,
                StageType.Djmax => 214,
                _ => (float)228,
            };
            plusCounts.transform.localPosition = new Vector3(x, Constants.COUNTS_POS.y, Constants.COUNTS_POS.z);

            GameStatsUtils.LockHighestScore();
            CountsTextManager.SetPlusCountsInstance(plusCounts);
            CountsTextManager.UpdatePlusCountsText();
        }
        catch (System.Exception e)
        {
            Melon<InfoPlusMod>.Logger.Error(e.ToString());
        }
    }
}

public enum StageType
{
    OtherEN,
    OtherCN,
    GC,
    Djmax,
    Unknown
}