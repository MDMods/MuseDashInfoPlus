using Il2CppGameLogic;
using HarmonyLib;

using MuseDashInfoPlus.Manager;

namespace MuseDashInfoPlus.Patches;

[HarmonyPatch(typeof(GameTouchPlay), nameof(GameTouchPlay.TouchTrigger))]
public class GameTouchPlayTouchTriggerPatch
{
    private static void Postfix(uint touchId)
    {
        if (touchId == 8)
            CountsTextManager.UpdatePlusCountsText();
    }
}