using Il2CppGameLogic;
using HarmonyLib;
using MelonLoader;

using MuseDashInfoPlus.Manager;

namespace MuseDashInfoPlus.Patches;

[HarmonyPatch(typeof(GameTouchPlay), nameof(GameTouchPlay.TouchTrigger))]
public class GameTouchPlayTouchTriggerPatch
{
    private static void Postfix(uint touchId)
    {
        try
        {
            CountsTextManager.UpdatePlusCountsText();
        }
        catch (System.Exception e)
        {
            Melon<InfoPlusMod>.Logger.Error(e.ToString());
        }
    }
}