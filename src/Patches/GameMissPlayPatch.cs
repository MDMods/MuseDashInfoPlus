using Il2CppGameLogic;
using JetBrains.Annotations;
using MDIP.Application.Services.Notes;

namespace MDIP.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
internal static class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
    {
        NoteEventService.HandleMissCube(idx, currentTick);
    }

    [UsedImplicitly] public static INoteEventService NoteEventService { get; set; }
}