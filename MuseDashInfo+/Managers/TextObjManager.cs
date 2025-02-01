using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MDIP.Managers;

public static class TextObjManager
{
    private static long _lastUpdateTick;
    public static GameObject TextLowerLeftObj { get; set; }
    public static GameObject TextLowerRightObj { get; set; }
    public static GameObject TextScoreBelowObj { get; set; }
    public static GameObject TextScoreRightObj { get; set; }
    public static GameObject TextUpperLeftObj { get; set; }
    public static GameObject TextUpperRightObj { get; set; }

    public static void UpdateAllText()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (now - _lastUpdateTick < Configs.Advanced.DataRefreshIntervalLimit) return;
        _lastUpdateTick = now;

        TextDataManager.UpdateValues();

        if (TextLowerLeftObj != null && Configs.TextFieldLowerLeft.Enabled)
            TextLowerLeftObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldLowerLeft.Text.UnEscapeReturn()));

        if (TextLowerRightObj != null && Configs.TextFieldLowerRight.Enabled)
            TextLowerRightObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldLowerRight.Text.UnEscapeReturn()));

        if (TextScoreBelowObj != null && Configs.TextFieldScoreBelow.Enabled)
            TextScoreBelowObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldScoreBelow.Text.UnEscapeReturn()));

        if (TextScoreRightObj != null && Configs.TextFieldScoreRight.Enabled)
            TextScoreRightObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldScoreRight.Text.UnEscapeReturn()));

        if (TextUpperLeftObj != null && Configs.TextFieldUpperLeft.Enabled)
            TextUpperLeftObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldUpperLeft.Text.UnEscapeReturn()));

        if (TextUpperRightObj != null && Configs.TextFieldUpperRight.Enabled)
            TextUpperRightObj.SetText(TextDataManager.GetFormattedText(Configs.TextFieldUpperRight.Text.UnEscapeReturn()));
    }

    private static void SetText(this GameObject obj, string text)
    {
        if (obj == null) return;
        var textComponent = obj.GetComponent<Text>();
        if (textComponent == null) return;
        textComponent.text = text;
    }

    public static void Reset()
    {
        Object.Destroy(TextLowerLeftObj);
        TextLowerLeftObj = null;
        Object.Destroy(TextLowerRightObj);
        TextLowerRightObj = null;
        Object.Destroy(TextScoreBelowObj);
        TextScoreBelowObj = null;
        Object.Destroy(TextScoreRightObj);
        TextScoreRightObj = null;
        Object.Destroy(TextUpperLeftObj);
        TextUpperLeftObj = null;
        Object.Destroy(TextUpperRightObj);
        TextUpperRightObj = null;
    }
}