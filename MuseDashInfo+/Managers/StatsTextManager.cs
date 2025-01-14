using UnityEngine;
using UnityEngine.UI;

using MuseDashInfoPlus.Utils;

namespace MuseDashInfoPlus.Manager;

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

    public static void SetHitStatsInstance(GameObject obj)
        => hitStatsText = obj.GetComponent<Text>();

    private static void SetHitStatsText(string text)
        => hitStatsText.text = text;

    public static void UpdateAllText()
    {
        if (gameStatsText == null) return;

        string text = ConfigManager.FinalGameStatsTextFormat;
        text = text.Replace("{acc}", GameStatsUtils.GetAccuracyString());
        text = text.Replace("{miss}", GameStatsUtils.GetMissCountsText());
        SetGameStatsText(text);

        text = ConfigManager.FinalScoreStatsTextFormat;
        text = text.Replace("{highest}", GameStatsUtils.HighestScore.ToString());
        text = text.Replace("{gap}", GameStatsUtils.GetScoreGapString());
        SetScoreStatsText(text);

        text = ConfigManager.FinalHitStatsTextFormat;
        text = text.Replace("{total}", GameStatsUtils.TotalCount.ToString());
        text = text.Replace("{hit}", GameStatsUtils.HitCount.ToString());
        SetHitStatsText(text);
    }

    public static void Reset()
    {
        gameStatsText = null;
        scoreStatsText = null;
        hitStatsText = null;
    }
}