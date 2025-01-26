using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using MelonLoader;
using System.Collections.Generic;
using System;

using MDIP.Utils;
using MDIP.Modules;

namespace MDIP.Managers;

public static class GameStatsManager
{
    private static StageBattleComponent _stage;
    private static TaskStageTarget _task;

    private static int _savedHighScore = 0;
    public static bool SavedHighScoreLocked = false;
    public static int SavedHighScore
    {
        get => _savedHighScore;
        set
        {
            if (value >= 0 && !SavedHighScoreLocked)
                _savedHighScore = value;
        }
    }

    private static readonly HashSet<float> SpecialValues = new() { 0.6f, 0.7f, 0.8f, 0.9f, 1f };
    private const float PRECISION = 0.0001f;
    private static HashSet<int> PlayedNoteIds = new();
    private static HashSet<int> MissedNoteIds = new();

    private static MusicData MashingNote;
    private static int MashedNum = -1;

    public record struct CurrentStats(
        int Perfect, int Great, int Early, int Late, int Music, int Energy, int Block, int RedPoint,
        int Score, bool IsHighScore = false
    );

    public record struct TotalStats(
        int Notes, int Hitable, int Monster, int Block, int Long,
        int Ghost, int Boss, int Energy, int Music, int Mul, int RedPoint
    );

    public record struct MissStats(
        int Monster, int Block, int Long, int LongPair,
        int Ghost, int Energy, int Music, int Mul, int RedPoint
    );

    private static CurrentStats _current;
    private static TotalStats _total;
    private static MissStats _miss;

    public static CurrentStats Current => _current;
    public static TotalStats Total => _total;
    public static MissStats Miss => _miss;

    // Calculated properties
    public static int MissCountHitable => _miss.Monster + _miss.Long + _miss.Ghost + _miss.Mul;
    public static int MissCountCollectable => _miss.Energy + _miss.Music + _miss.RedPoint;
    public static int MissCount => MissCountHitable + MissCountCollectable + _miss.Block;

    public static float AccuracyTotal => _total.Music + _total.Energy + _total.Hitable + _total.Block;
    public static float AccuracyCounted => _current.Perfect + (_current.Great / 2f) +
        _current.Block + _current.Music + _current.Energy + _current.RedPoint;
    public static float AccuracyRest => Math.Max(0, GetAccuracyRest());
    public static float GetAccuracyRest() => AccuracyTotal - _current.Perfect - _current.Great -
        _current.Block - _current.Music - _current.Energy - _current.RedPoint -
        _miss.Music - _miss.Energy - MissCountHitable - _miss.LongPair - _miss.Block;

    public static bool IsInGame => _task != null && _stage != null;
    public static bool IsAllPerfect => IsInGame && _current.Great + MissCount < 1;
    public static bool IsTruePerfect => IsAllPerfect && _current.Early + _current.Late < 1;

    public static void UpdateCurrentStats()
    {
        if (_task == null) return;
        _current = new CurrentStats(
            _task.m_PerfectResult,
            _task.m_GreatResult,
            _current.Early,
            _current.Late,
            _task.m_MusicCount,
            _task.m_EnergyCount,
            _current.Block,
            _task.m_RedPoint,
            _task.m_Score,
            _task.m_Score > _savedHighScore
        );

        if (Helper.OutputAccuracyCalculationDatas)
        {
            Melon<MDIPMod>.Logger.Warning($"=========== Accuracy Stats ===========");
            Melon<MDIPMod>.Logger.Msg($"Total:{AccuracyTotal} | Counted:{AccuracyCounted} | Rest:{AccuracyRest}");
            Melon<MDIPMod>.Logger.Msg($"Total => Music:{Total.Music} | Energy:{Total.Energy} | Block:{Total.Block} | Hitable:{Total.Hitable}");
            Melon<MDIPMod>.Logger.Msg($"Counted => Music:{Current.Music} | Energy:{Current.Energy} | Block:{Current.Block} Perfect:{Current.Perfect} | Great:{Current.Great} /2f | | RedPoint:{Current.RedPoint}");
            Melon<MDIPMod>.Logger.Msg($"Miss => Music:{Miss.Music} | Energy:{Miss.Energy} | Block:{Miss.Block} | Hitable:{MissCountHitable} | LongPair:{Miss.LongPair}");
            Melon<MDIPMod>.Logger.Msg($"{AccuracyTotal} - {Current.Perfect + Current.Great + Current.Block + Current.Music + Current.Energy + Current.RedPoint} - {Miss.Music + Miss.Energy + MissCountHitable + Miss.LongPair + Miss.Block} = {AccuracyRest}");
            Melon<MDIPMod>.Logger.Warning($"======================================");
            Melon<MDIPMod>.Logger.Msg($"Calc Acc: {GetCalculatedAccuracy()} | True Acc:{GetTrueAccuracy()}");
            Melon<MDIPMod>.Logger.Warning($"======================================");
        }
    }

    public static float GetTrueAccuracy() => _task.GetAccuracy() * 100f;

    public static float GetCalculatedAccuracy()
    {
        float acc = Configs.Main.AccuracyDisplayMode == 2
                ? AccuracyCounted / (AccuracyTotal - AccuracyRest)
                : (AccuracyCounted + AccuracyRest) / AccuracyTotal;

        float rounded = MathF.Round(acc / PRECISION) * PRECISION;
        return (acc < rounded && SpecialValues.Contains(rounded) ?
            rounded - PRECISION : rounded) * 100f;
    }

    public static string FormatOverview()
    {
        if (IsTruePerfect) return Constants.TEXT_TRUE_PERFECT.Colored(Constants.COLOR_RANK_TP);
        if (IsAllPerfect) return Constants.TEXT_ALL_PERFECT.Colored(Constants.COLOR_RANK_AP);
        return FormatAccuracy();
    }

    public static string FormatAccuracy()
    {
        if (IsAllPerfect) return "100%".Colored(IsTruePerfect ? Constants.COLOR_RANK_TP : Constants.COLOR_RANK_AP);
        var acc = GetCalculatedAccuracy();
        string color = IsTruePerfect ? Constants.COLOR_RANK_TP : acc switch
        {
            >= 100f => Constants.COLOR_RANK_AP,
            >= 95f => Constants.COLOR_RANK_SS,
            >= 90f => Constants.COLOR_RANK_S,
            >= 80f => Constants.COLOR_RANK_A,
            >= 70f => Constants.COLOR_RANK_B,
            >= 60f => Constants.COLOR_RANK_C,
            _ => Constants.COLOR_RANK_D
        };
        return $"{acc:F2}%".Colored(color);
    }

    public static string FormatScoreGap()
    {
        int gap = _current.Score - _savedHighScore;
        if (gap == 0) return string.Empty;

        var (color, prefix) = gap > 0
            ? (Configs.Main.ScoreGapAheadColor, "+")
            : (Configs.Main.ScoreGapBehindColor, "");

        return Math.Abs(gap) < 1000
            ? $"{prefix}{gap}".Colored(color)
            : $"{prefix}{gap / 1000}K".Colored(color);
    }

    public static string FormatStats()
    {
        var parts = new List<string>();

        if (IsAllPerfect)
        {
            if (IsTruePerfect)
                return string.Empty;

            if (Configs.Main.ShowEarlyLateCounts)
            {
                if (_current.Early > 0)
                    parts.Add($"{_current.Early}E".Colored(Configs.Main.EarlyCountsColor));
                if (_current.Late > 0)
                    parts.Add($"{_current.Late}L".Colored(Configs.Main.LateCountsColor));
            }
        }
        else
        {
            if (_current.Great > 0)
                parts.Add($"{_current.Great}G".Colored(Configs.Main.GreatCountsColor));
            if (MissCountHitable + _miss.Block > 0)
                parts.Add($"{MissCountHitable + _miss.Block}M".Colored(Configs.Main.NormalMissCountsColor));
            if (!Utils.GameUtils.IsSpellMode && MissCountCollectable > 0)
                parts.Add($"{MissCountCollectable}H".Colored(Configs.Main.CollectableMissCountsColor));

            if (Configs.Main.EarlyLateCountsDisplayMode == 2)
            {
                if (_current.Early > 0)
                    parts.Add($"{_current.Early}E".Colored(Configs.Main.EarlyCountsColor));
                if (_current.Late > 0)
                    parts.Add($"{_current.Late}L".Colored(Configs.Main.LateCountsColor));
            }
        }

        return string.Join(" ", parts);
    }

    public static void AddEarly() => _current.Early++;
    public static void AddLate() => _current.Late++;

    public static void CountNote(int id, CountNoteAction action, int doubleId = -1, bool isLongStart = false)
    {
        switch (action)
        {
            case CountNoteAction.Block:
                if (PlayedNoteIds.Add(id))
                    _current.Block++;
                break;
            case CountNoteAction.MissMonster:
                if (doubleId == -1)
                {
                    if (MissedNoteIds.Add(id))
                        _miss.Monster++;
                }
                else
                {
                    if (MissedNoteIds.Add(id) && MissedNoteIds.Add(doubleId))
                        _miss.Monster += 2;
                }
                break;
            case CountNoteAction.MissBlock:
                if (MissedNoteIds.Add(id))
                    _miss.Block++;
                if (!PlayedNoteIds.Add(id))
                    _current.Block--;
                break;
            case CountNoteAction.MissLong:
                if (MissedNoteIds.Add(id))
                {
                    _miss.Long++;
                    if (isLongStart) _miss.LongPair++;
                }
                break;
            case CountNoteAction.MissGhost:
                if (MissedNoteIds.Add(id))
                    _miss.Ghost++;
                break;
            case CountNoteAction.MissEnergy:
                if (MissedNoteIds.Add(id))
                    _miss.Energy++;
                break;
            case CountNoteAction.MissMusic:
                if (MissedNoteIds.Add(id))
                    _miss.Music++;
                break;
            case CountNoteAction.MissMul:
                if (MissedNoteIds.Add(id))
                {
                    ResetMashing();
                    _miss.Mul++;
                }
                break;
        }
    }

    public static void ResetMashing()
    {
        MashingNote = null;
        MashedNum = -1;
    }

    public static void CheckMashing()
    {
        if (MashingNote == null) return;
        bool timesup = _stage.realTimeTick > (MashingNote.tick + MashingNote.configData.length) * 1000;
        if (timesup)
        {
            bool reachHigh = MashedNum >= MashingNote.GetMulHitHighThreshold();
            if (!reachHigh && !MissedNoteIds.Contains(int.Parse(MashingNote.noteData.id)))
                _miss.Mul++;
            ResetMashing();
        }
    }

    public static void Mashing(MusicData note)
    {
        if (!MissedNoteIds.Contains(int.Parse(note.noteData.id)))
        {
            MashingNote = note;
            MashedNum++;
            if (MashedNum < note.GetMulHitHighThreshold())
                return;
        }
        ResetMashing();
    }

    public static void Init()
    {
        try
        {
            _stage = Singleton<StageBattleComponent>.instance;
            _task = Singleton<TaskStageTarget>.instance;
            _savedHighScore = Math.Max(BattleHelper.GetCurrentMusicHighScore(), SavedHighScore);

            foreach (var note in _stage.GetMusicData())
            {
                var type = (NoteType)note.noteData.type;
                if (!note.isLongPressing)
                {
                    if (note.noteData.addCombo) _total.Hitable++;
                    if (type.IsRegularNote()) _total.Notes++;
                }

                switch (type)
                {
                    case NoteType.Monster: _total.Monster++; break;
                    case NoteType.Block: _total.Block++; break;
                    case NoteType.Long when !note.isLongPressing: _total.Long++; break;
                    case NoteType.Ghost: _total.Ghost++; break;
                    case NoteType.Boss: _total.Boss++; break;
                    case NoteType.Energy: _total.Energy++; break;
                    case NoteType.Music: _total.Music++; break;
                    case NoteType.Mul: _total.Mul++; break;
                }
            }

            SavedHighScoreLocked = false;
        }
        catch (Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }

    public static void Reset()
    {
        _stage = null;
        _task = null;

        _current = default;
        _total = default;
        _miss = default;

        ResetMashing();

        SavedHighScore = 0;
        PlayedNoteIds.Clear();
        MissedNoteIds.Clear();
    }
}