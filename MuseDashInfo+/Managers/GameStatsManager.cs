using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using System.Collections.Generic;
using System;
using MelonLoader;

using MDIP.Utils;
using MDIP.Patches;

namespace MDIP.Managers;

public static class GameStatsManager
{
    private static StageBattleComponent _stage;
    private static TaskStageTarget _task;

    // From vanilla
    public static int CurrentScore => _task?.m_Score ?? 0;
    public static int CurPerfectCount => _task?.m_PerfectResult ?? 0;
    public static int CurGreatCount => _task?.m_GreatResult ?? 0;
    public static int CurMusicCount => _task?.m_MusicCount ?? 0;
    public static int CurEnergyCount => _task?.m_EnergyCount ?? 0;
    public static int CurRedPointCount => _task?.m_RedPoint ?? 0;
    // _task.m_Blood & _task.m_Block did not work correctly

    // From mod
    public static int CurBlockCount { get; private set; }

    public static int CurHitableCount => CurPerfectCount + CurGreatCount;
    public static int CurCollectableCount => CurEnergyCount + CurMusicCount;
    public static int CurCountedCount => CurHitableCount + CurCollectableCount + CurBlockCount;

    public static int TotalNoteCount { get; private set; }
    public static int TotalHitableCount { get; private set; }
    public static int TotalMonsterCount { get; private set; }
    public static int TotalBlockCount { get; private set; }
    public static int TotalLongCount { get; private set; }
    public static int TotalGhostCount { get; private set; }
    public static int TotalBossCount { get; private set; }
    public static int TotalEnergyCount { get; private set; }
    public static int TotalMusicCount { get; private set; }
    public static int TotalMulCount { get; private set; }
    public static int TotalRedPointCount { get; private set; }

    public static int MissMonsterCount { get; private set; }
    public static int MissBlockCount { get; private set; }
    public static int MissLongCount { get; private set; }
    public static int MissLongPairCount { get; private set; }
    public static int MissGhostCount { get; private set; }
    public static int MissEnergyCount { get; private set; }
    public static int MissMusicCount { get; private set; }
    public static int MissRedPointCount { get; private set; } // TODO
    public static int MissHitableCount => MissMonsterCount + MissLongCount + MissGhostCount;
    public static int MissCollectableCount => MissEnergyCount + MissMusicCount + MissRedPointCount;
    public static int MissCount => MissHitableCount + MissCollectableCount + MissBlockCount;

    public static float MDAccTotal => TotalMusicCount + TotalEnergyCount + TotalHitableCount + TotalBlockCount;
    public static float MDAccCounted => CurPerfectCount + (CurGreatCount / 2f) + CurBlockCount + CurMusicCount + CurEnergyCount + CurRedPointCount;
    public static float MDAccRest => MDAccTotal - CurPerfectCount - CurGreatCount - CurBlockCount - CurMusicCount - CurEnergyCount - CurRedPointCount
                 - MissMusicCount - MissEnergyCount - MissHitableCount - MissLongPairCount - MissBlockCount;

    public static int SavedHighestScore { private get; set; }
    public static int HighestScore { get; private set; }
    public static int ScoreGap => CurrentScore - HighestScore;

    private static List<int> CountedNoteIdList = new();
    private static List<int> CountedBindEnergyIdList = new();

    private static readonly HashSet<float> SpecialValues = new() { 0.6f, 0.7f, 0.8f, 0.9f, 1f };
    private const float PRECISION = 0.0001f;

    public static float GetTrueAccuracy() => _task?.GetAccuracy() * 100 ?? 0;

    public static float GetCalculatedAccuracy()
    {
        float acc = (MDAccCounted + MDAccRest) / MDAccTotal;
        float roundedAcc = MathF.Round(acc / PRECISION) * PRECISION;

        if (acc < roundedAcc && SpecialValues.Contains(roundedAcc))
            roundedAcc -= PRECISION;

        return roundedAcc * 100f;
    }

    public static string GetAccuracyString()
    {
        var acc = GetCalculatedAccuracy();
        string color = acc >= 100f ? "#fff000" // SSS
            : acc >= 95f ? "#ccf0fe" // SS
            : acc >= 90f ? "#ff0089" // S
            : acc >= 80f ? "#ad00ff" // A
            : acc >= 70f ? "#00bbff" // B
            : acc >= 60f ? "#00ff23" // C
            : "#a2a2a2"; // D
        return $"<color={color}>{acc:F2}%</color>";
    }

    public static string GetScoreGapString()
    {
        if (ScoreGap == 0) return string.Empty;
        bool ahead = ScoreGap > 0;
        string str = ScoreGap < 1000 && ScoreGap > -1000
            ? $"<color={(ahead ? Constants.GAP_AHEAD_COLOR : Constants.GAP_BEHIND_COLOR)}>{(ahead ? "+" : string.Empty)}{ScoreGap}</color>"
            : $"<color={(ahead ? Constants.GAP_AHEAD_COLOR : Constants.GAP_BEHIND_COLOR)}>{(ahead ? "+" : string.Empty)}{ScoreGap / 1000}K</color>";
        return str;
    }

    public static string GetMissCountsString()
        => (CurGreatCount + MissCount) == 0 ? "AP"
        : (CurGreatCount < 1 ? string.Empty : $"{CurGreatCount}G")
        + ((MissHitableCount + MissBlockCount) < 1 ? string.Empty : $" {MissHitableCount + MissBlockCount}M")
        + ((PnlBattleGameStartPatch.IsSpellMode ? 0 : MissCollectableCount) < 1 ? string.Empty : $" {MissCollectableCount}H");

    public static void LockHighestScore()
    {
        HighestScore = SavedHighestScore;
        SavedHighestScore = -1;
    }

    public static void DecideConstantDatas()
    {
        try
        {
            int curHiScore = HighestScore;
            HighestScore = BattleHelper.GetCurrentMusicHighScore() <= 0
                ? curHiScore > 0 ? curHiScore : 0
                : BattleHelper.GetCurrentMusicHighScore();

            foreach (var note in _stage.GetMusicData())
            {
                var type = (Modules.NoteType)note.noteData.type;
                switch (type)
                {
                    case Modules.NoteType.Monster:
                        TotalMonsterCount++;
                        break;
                    case Modules.NoteType.Block:
                        TotalBlockCount++;
                        break;
                    case Modules.NoteType.Long when !note.isLongPressing:
                        TotalLongCount++;
                        break;
                    case Modules.NoteType.Ghost:
                        TotalGhostCount++;
                        break;
                    case Modules.NoteType.Boss:
                        TotalBossCount++;
                        break;
                    case Modules.NoteType.Energy:
                        TotalEnergyCount++;
                        break;
                    case Modules.NoteType.Music:
                        TotalMusicCount++;
                        break;
                    case Modules.NoteType.Mul:
                        TotalMulCount++;
                        break;
                }
                if (note.noteData.addCombo && !note.isLongPressing) TotalHitableCount++;
                if (type.IsRegularNote() && !note.isLongPressing) TotalNoteCount++;
            }
        }
        catch (Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }

    public static void AddBlockCur(int id)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        CurBlockCount++;
    }

    public static void AddMonsterMiss(int id, int doubleId = -1)
    {
        if (CountedNoteIdList.Contains(id) || CountedNoteIdList.Contains(doubleId)) return;
        if (doubleId != -1)
        {
            // Double counted as 2 misses
            // And ignore if there is an another call in
            CountedNoteIdList.Add(doubleId);
            MissMonsterCount++;
        }
        CountedNoteIdList.Add(id);
        MissMonsterCount++;
    }

    public static void AddBlockMiss(int id)
    {
        if (CountedNoteIdList.Contains(id))
        {
            CurBlockCount--;
            MissBlockCount++;
        }
        else
        {
            CountedNoteIdList.Add(id);
            MissBlockCount++;
        }
    }

    public static void AddLongMiss(int id, bool isStart = false)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        MissLongCount++;
        if (isStart) MissLongPairCount++;
    }

    public static void AddGhostMiss(int id)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        MissGhostCount++;
    }

    public static void AddEnergyMiss(int id)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        MissEnergyCount++;
    }

    public static void AddBindEnergyMiss(int id)
    {
        if (CountedBindEnergyIdList.Contains(id)) return;
        CountedBindEnergyIdList.Add(id);
        MissEnergyCount++;
    }

    public static void AddMusicMiss(int id)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        MissMusicCount++;
    }

    public static void Reload()
    {
        _stage = Singleton<StageBattleComponent>.instance;
        _task = Singleton<TaskStageTarget>.instance;
    }

    public static void Reset()
    {
        _stage = null;
        _task = null;

        CurBlockCount = 0;

        MissMonsterCount = 0;
        MissBlockCount = 0;
        MissLongCount = 0;
        MissLongPairCount = 0;
        MissGhostCount = 0;
        MissEnergyCount = 0;
        MissMusicCount = 0;
        MissEnergyCount = 0;
        MissRedPointCount = 0;

        TotalNoteCount = 0;
        TotalHitableCount = 0;
        TotalMonsterCount = 0;
        TotalBlockCount = 0;
        TotalLongCount = 0;
        TotalGhostCount = 0;
        TotalBossCount = 0;
        TotalEnergyCount = 0;
        TotalMusicCount = 0;
        TotalMulCount = 0;

        SavedHighestScore = 0;

        CountedNoteIdList = new();
        CountedBindEnergyIdList = new();
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Utils/GameUtils.cs