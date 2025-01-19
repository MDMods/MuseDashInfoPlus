using UnityEngine;
using UnityEngine.UI;

using MDIP.Managers;

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
        GameStatsManager.UpdateCurrentStats();
        if (gameStatsText != null)
        {
            string text = ConfigManager.FinalGameStatsTextFormat;
            text = text.Replace("{acc}", GameStatsManager.FormatAccuracy());
            text = text.Replace("{miss}", GameStatsManager.FormatMissCounts());
            SetGameStatsText(text);
        }
        if (scoreStatsText != null)
        {
            string text = ConfigManager.FinalScoreStatsTextFormat;
            text = text.Replace("{highest}", GameStatsManager.SavedHighScore.ToString());
            text = text.Replace("{gap}", GameStatsManager.FormatScoreGap());
            SetScoreStatsText(text);
        }
        if (hitStatsText != null)
        {
            string text = ConfigManager.FinalNoteStatsTextFormat;
            text = text.Replace("{total}", ((int)GameStatsManager.AccuracyTotal).ToString());
            text = text.Replace("{hit}", ((int)GameStatsManager.AccuracyCounted).ToString());
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