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
        SetPlusCountsText($"<size=28>{GameCountsUtils.GetHitCountText()}</size>\n{GameCountsUtils.GetMissCountText()}");
    }

    public static void Reset()
    {
        plusCountsText = null;
    }
}