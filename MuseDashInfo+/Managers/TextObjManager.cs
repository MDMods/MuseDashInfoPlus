using System;
using UnityEngine;
using UnityEngine.UI;

using MDIP.Utils;

namespace MDIP.Managers;

public static class TextObjManager
{
    public static GameObject TextLowerLeftObj { get; set; }
    public static GameObject TextLowerRightObj { get; set; }
    public static GameObject TextScoreBelowObj { get; set; }
    public static GameObject TextScoreRightObj { get; set; }
    public static GameObject TextUpperLeftObj { get; set; }
    public static GameObject TextUpperRightObj { get; set; }

    private static long lastUpdateTick = 0;
    public static void UpdateAllText()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (now - lastUpdateTick < 123) return;
        lastUpdateTick = now;

        GameStatsManager.UpdateCurrentStats();

        if (TextLowerLeftObj != null && Configs.TextFieldLowerLeft.Enabled)
            TextLowerLeftObj.SetText(FormatText(Configs.TextFieldLowerLeft.Text));

        if (TextLowerRightObj != null && Configs.TextFieldLowerRight.Enabled)
            TextLowerRightObj.SetText(FormatText(Configs.TextFieldLowerRight.Text));

        if (TextScoreBelowObj != null && Configs.TextFieldScoreBelow.Enabled)
            TextScoreBelowObj.SetText(FormatText(Configs.TextFieldScoreBelow.Text));

        if (TextScoreRightObj != null && Configs.TextFieldScoreRight.Enabled)
            TextScoreRightObj.SetText(FormatText(Configs.TextFieldScoreRight.Text));

        if (TextUpperLeftObj != null && Configs.TextFieldUpperLeft.Enabled)
            TextUpperLeftObj.SetText(FormatText(Configs.TextFieldUpperLeft.Text));

        if (TextUpperRightObj != null && Configs.TextFieldUpperRight.Enabled)
            TextUpperRightObj.SetText(FormatText(Configs.TextFieldUpperRight.Text));
    }

    private static void SetText(this GameObject obj, string text)
    {
        if (obj == null) return;
        var textComponent = obj.GetComponent<Text>();
        if (textComponent == null) return;
        textComponent.text = text;
    }

    private static string FormatText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        text = text.Replace("{acc}", GameStatsManager.FormatAccuracy());
        text = text.Replace("{stats}", GameStatsManager.FormatStats());
        text = text.Replace("{highest}", GameStatsManager.SavedHighScore.ToString());
        text = text.Replace("{gap}", GameStatsManager.FormatScoreGap());
        text = text.Replace("{total}", ((int)GameStatsManager.AccuracyTotal).ToString());
        text = text.Replace("{hit}", ((int)GameStatsManager.AccuracyCounted).ToString());
        text = text.Replace("{song}", GameUtils.MusicName);
        text = text.Replace("{diff}", GameUtils.MusicDiffStr);
        text = text.Replace("{level}", GameUtils.MusicLevel);
        return text;
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