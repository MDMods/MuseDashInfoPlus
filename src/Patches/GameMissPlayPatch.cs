using Il2CppGameLogic;

namespace MDIP.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
internal class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
    {
        NoteEventManager.HandleMissCube(idx, currentTick);
    }
}