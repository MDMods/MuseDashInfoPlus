using Il2CppAssets.Scripts.GameCore.HostComponent;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.RuntimeData;
using MDIP.Application.Services.Scoped.UI;
using MDIP.Core.Utilities;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
internal static class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
    {
        if (VictoryScreenService != null)
        {
            VictoryScreenService.OnSetDetailInfo(__instance);
            return;
        }

        // Fallback: save personal best when scoped service is unavailable (e.g. scope disposed during scene transition)
        SavePersonalBestDirect();
    }

    private static void SavePersonalBestDirect()
    {
        if (RuntimeDataStore == null)
            return;

        var task = TaskStageTarget.instance;
        if (task == null)
            return;

        var hash = MusicInfoUtils.CurMusicHash;
        var acc = (float)Math.Round((double)(task.GetAccuracy() * 100f), 2);
        var score = task.m_Score;

        RuntimeDataStore.StorePersonalBestAccuracy(hash, acc);
        RuntimeDataStore.StorePersonalBestScore(hash, score);
    }

    [UsedImplicitly] [Inject] public static IVictoryScreenService VictoryScreenService { get; set; }
    [UsedImplicitly] [Inject] public static IRuntimeDataStore RuntimeDataStore { get; set; }
}