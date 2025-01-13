using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;

namespace MuseDashInfoPlus.Utils;

public static class GameStatsUtils
{
    private static StageBattleComponent _stage;
    private static TaskStageTarget _task;

    public static bool Playing => _stage?.isInGame ?? false;

    // Normal judgements
    public static int TotalCount => _task?.GetTotalNum() ?? 0;
    public static int HitCount => _task?.m_HitCount ?? 0;
    public static int PerfectCount => _task?.m_PerfectResult ?? 0;
    public static int GreatCount => _task?.m_GreatResult ?? 0;
    public static int NoteCount => _task?.m_MusicCount ?? 0;
    public static int HeartCount => _task?.m_Blood ?? 0;
    public static int MissCount => NormalMissCount + GhostMissCount;

    // From patches
    public static int JumpOverCount { get; internal set; }
    public static int NormalMissCount { get; internal set; }
    public static int GhostMissCount { get; internal set; }
    public static int CollectableMissCount { get; internal set; }

    internal static void Reload()
    {
        _stage = Singleton<StageBattleComponent>.instance;
        _task = Singleton<TaskStageTarget>.instance;
    }

    internal static void Reset()
    {
        _stage = null;
        _task = null;

        JumpOverCount = 0;
        NormalMissCount = 0;
        GhostMissCount = 0;
        CollectableMissCount = 0;
    }

    public static string GetMissCountsText()
        => (GreatCount + MissCount + CollectableMissCount) == 0 ? "AP"
        : (GreatCount < 1 ? string.Empty : $"{GreatCount}G")
        + (MissCount < 1 ? string.Empty : $" {MissCount}M")
        + (CollectableMissCount < 1 ? string.Empty : $" {CollectableMissCount}H");
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Utils/GameUtils.cs