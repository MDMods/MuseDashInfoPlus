using UnityEngine;
using UnityEngine.UI;

using MDIP.Utils;

namespace MDIP.Manager;

public static class StatsTextManager
{
    private static Text gameStatsText;
    private static Text scoreStatsText;
    private static Text hitStatsText;

    public static void SetGameStatsInstance(GameObject obj)
        => gameStatsText = obj.GetComponent<Text>();

    private static void SetGameStatsText(string text)
        => gameStatsText.text = text;

    public static void SetScoreStatsInstance(GameObject obj)
        => scoreStatsText = obj.GetComponent<Text>();

    private static void SetScoreStatsText(string text)
        => scoreStatsText.text = text;

    public static void SetNoteStatsInstance(GameObject obj)
        => hitStatsText = obj.GetComponent<Text>();

    private static void SetNoteStatsText(string text)
        => hitStatsText.text = text;

    public static void UpdateAllText()
    {
        if (gameStatsText != null)
        {
            string text = ConfigManager.FinalGameStatsTextFormat;
            text = text.Replace("{acc}", GameStatsUtils.GetAccuracyString());
            text = text.Replace("{miss}", GameStatsUtils.GetMissCountsText());
            SetGameStatsText(text);
        }
        if (scoreStatsText != null)
        {
            string text = ConfigManager.FinalScoreStatsTextFormat;
            text = text.Replace("{highest}", GameStatsUtils.HighestScore.ToString());
            text = text.Replace("{gap}", GameStatsUtils.GetScoreGapString());
            SetScoreStatsText(text);
        }
        if (hitStatsText != null)
        {
            string text = ConfigManager.FinalNoteStatsTextFormat;
            text = text.Replace("{total}", GameStatsUtils.TotalCount.ToString());
            text = text.Replace("{hit}", (GameStatsUtils.HitCount + GameStatsUtils.HeartCount + GameStatsUtils.MusicNoteCount + GameStatsUtils.JumpOverCount).ToString());
            SetNoteStatsText(text);
        }
    }

    public static void Reset()
    {
        gameStatsText = null;
        scoreStatsText = null;
        hitStatsText = null;
    }
}