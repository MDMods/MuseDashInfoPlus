using UnityEngine;
using UnityEngine.UI;

using MuseDashInfoPlus.Utils;

namespace MuseDashInfoPlus.Manager;

public static class CountsTextManager
{
    private static Text plusCountsText;

    public static void SetPlusCountsInstance(GameObject obj)
    {
        plusCountsText = obj.GetComponent<Text>();
    }

    private static void SetPlusCountsText(string text)
    {
        plusCountsText.text = text;
    }

    public static void UpdatePlusCountsText()
    {
        if (plusCountsText == null) return;
        string text = InfoPlusMod.FinalCountsTextFormat;
        text = text.Replace("{total}", GameStatsUtils.TotalCount.ToString());
        text = text.Replace("{hit}", GameStatsUtils.HitCount.ToString());
        text = text.Replace("{miss}", GameStatsUtils.GetMissCountsText());
        text = text.Replace("{acc}", GameStatsUtils.Accuracy.ToString("F2") + '%');
        text = text.Replace("{highest}", GameStatsUtils.HighestScore.ToString());
        text = text.Replace("{gap}", GameStatsUtils.GetScoreGapString());
        SetPlusCountsText(text);
    }

    public static void Reset()
    {
        plusCountsText = null;
    }
}