using Il2CppGameLogic;
using JetBrains.Annotations;
using MDIP.Application.Contracts;

namespace MDIP.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
internal static class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
    {
        NoteEventService.HandleMissCube(idx, currentTick);
    }

    #region Injections

    [UsedImplicitly] public static INoteEventService NoteEventService { get; set; }

    #endregion
}