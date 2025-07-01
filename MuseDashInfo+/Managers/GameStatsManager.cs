using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using GameUtils = MDIP.Utils.GameUtils;

namespace MDIP.Managers;

public static class GameStatsManager
{
    private const float Precision = 0.0001f;
    private static BattleRoleAttributeComponent _role;
    private static StageBattleComponent _stage;
    private static TaskStageTarget _task;

    private static readonly HashSet<float> SpecialValues = [0.6f, 0.7f, 0.8f, 0.9f, 1f];

    private static CurrentStats _current;
    private static TotalStats _total;
    private static MissStats _miss;
    private static HistoryStats _history;
    public static bool IsInGame { get; set; }
    public static bool IsFirstTry { get; set; } = true;

    private static Dictionary<short, (MusicData, int)> MashingNotes { get; } = new();
    private static Dictionary<short, int> MasherGreatRecords { get; } = new();
    private static HashSet<short> PlayedNoteIds { get; } = [];
    private static HashSet<short> MissedNoteIds { get; } = [];
    public static int CurrentSkySpeed { get; set; }
    public static int CurrentGroundSpeed { get; set; }

    public static float StoredHighestAccuracy { get; set; }
    public static int StoredHighestScore { get; set; }

    public static CurrentStats Current => _current;
    public static TotalStats Total => _total;
    public static MissStats Miss => _miss;
    public static HistoryStats History => _history;

    // Calculated properties
    public static int MissCountHittable => _miss.Monster + _miss.Long + _miss.Mul;
    public static int MissCountCollectible => _miss.Energy + _miss.Music + _miss.RedPoint + _miss.Ghost;
    public static int MissCount => MissCountHittable + MissCountCollectible + _miss.Block;

    public static float AccuracyCalculationTotal => _total.Music + _total.Energy + _total.Hittable + _total.Block;

    public static float AccuracyCalculationCounted => _current.Perfect + _current.Great / 2f +
                                                      _current.Block + _current.Music + _current.Energy + _current.RedPoint;

    public static float AccuracyCalculationRest => Math.Max(0, GetAccuracyRest());

    public static bool IsAvailable => _stage != null && _task != null && _role != null;
    public static bool IsAllPerfect => IsInGame && _current.Great + MissCount < 1;
    public static bool IsTruePerfect => IsAllPerfect && _current.Early + _current.Late < 1;

    public static float GetAccuracyRest() => AccuracyCalculationTotal - _current.Perfect - _current.Great -
                                             _current.Block - _current.Music - _current.Energy - _current.RedPoint -
                                             MissCount - _miss.LongPair;

    public static void UpdateCurrentStats()
    {
        if (!IsAvailable) return;

        _current = new(
            _task.m_PerfectResult,
            _task.m_GreatResult,
            _role.early,
            _role.late,
            _task.m_MusicCount,
            _task.m_EnergyCount,
            _current.Block,
            _task.m_RedPoint,
            _task.m_Score
        );

        if (Configs.Advanced.DisplayNoteDebuggingData)
            OutputCurrentNoteDebuggingData(true);
    }

    public static void OutputCurrentNoteDebuggingData(bool isInGame)
    {
        if (isInGame) Melon<MDIPMod>.Logger.Warning($"=========== Tick: {_stage?.realTimeTick ?? -1} | CurId: {_stage?.curTimeNodeOrder?.idx ?? -1} ===========");
        else Melon<MDIPMod>.Logger.Error("=========== Note Count Error ===========");
        Melon<MDIPMod>.Logger.Msg($"Total:{AccuracyCalculationTotal} | Counted:{AccuracyCalculationCounted} | Rest:{AccuracyCalculationRest}");
        Melon<MDIPMod>.Logger.Msg($"Total => Music:{Total.Music} | Energy:{Total.Energy} | Block:{Total.Block} | RedPoint:{Total.RedPoint} | Hittable:{Total.Hittable}");
        Melon<MDIPMod>.Logger.Msg($"Counted => Music:{Current.Music} | Energy:{Current.Energy} | Block:{Current.Block} | RedPoint:{Current.RedPoint} | Perfect:{Current.Perfect} | Great:{Current.Great} /2f");
        Melon<MDIPMod>.Logger.Msg($"Miss => Music:{Miss.Music} | Energy:{Miss.Energy} | Block:{Miss.Block} | RedPoint:{Miss.RedPoint} | Hittable:{MissCountHittable} | LongPair:{Miss.LongPair} | Ghost:{Miss.Ghost}");
        Melon<MDIPMod>.Logger.Msg(
            $"{AccuracyCalculationTotal} - {Current.Perfect + Current.Great + Current.Block + Current.Music + Current.Energy + Current.RedPoint} - {Miss.Music + Miss.Energy + MissCountHittable + Miss.LongPair + Miss.Block + Miss.RedPoint + Miss.Ghost} = {AccuracyCalculationRest}");
        if (!isInGame) OutputDividingLine(true);
        Melon<MDIPMod>.Logger.Msg($"Calc Acc: {GetCalculatedAccuracy()} | True Acc:{GetTrueAccuracy()}");
        if (!isInGame) OutputDividingLine(true);
    }

    private static void OutputDividingLine(bool error)
    {
        if (error) Melon<MDIPMod>.Logger.Error("======================================");
        else Melon<MDIPMod>.Logger.Warning("======================================");
    }

    public static void UpdateCurrentSpeed(bool isAir, int speed)
    {
        if (isAir) CurrentSkySpeed = speed;
        else CurrentGroundSpeed = speed;
    }

    public static float GetTrueAccuracy() => _task.GetAccuracy() * 100f;

    public static float GetCalculatedAccuracy(int mode = -1)
    {
        if (mode < 1) mode = Configs.Main.AccuracyDisplayMode;

        var acc = mode == 2
            ? AccuracyCalculationCounted / (AccuracyCalculationTotal - AccuracyCalculationRest)
            : (AccuracyCalculationCounted + AccuracyCalculationRest) / AccuracyCalculationTotal;

        var rounded = MathF.Round(acc / Precision) * Precision;
        return (acc < rounded && SpecialValues.Contains(rounded)
                ? rounded - Precision
                : rounded
            ) * 100f;
    }

    public static string FormatOverview()
    {
        if (IsTruePerfect) return Configs.Main.TextTruePerfect.Colored(Configs.Main.RankTPColor);
        return IsAllPerfect ? Configs.Main.TextAllPerfect.Colored(Configs.Main.RankAPColor) : FormatAccuracy();
    }

    public static string FormatRank()
        => IsTruePerfect ? Configs.Main.TextTruePerfect.Colored(Configs.Main.RankTPColor) :
            IsAllPerfect ? Configs.Main.TextAllPerfect.Colored(Configs.Main.RankAPColor) :
            GetCalculatedAccuracy() switch
            {
                >= 95f => "SS".Colored(Configs.Main.RankSSColor),
                >= 90f => "S".Colored(Configs.Main.RankSColor),
                >= 80f => "A".Colored(Configs.Main.RankAColor),
                >= 70f => "B".Colored(Configs.Main.RankBColor),
                >= 60f => "C".Colored(Configs.Main.RankCColor),
                _ => "D".Colored(Configs.Main.RankDColor)
            };

    public static string FormatAccuracy()
    {
        if (IsAllPerfect) return "100%".Colored(IsTruePerfect ? Configs.Main.RankTPColor : Configs.Main.RankAPColor);
        var acc = GetCalculatedAccuracy();
        var color = IsTruePerfect ?
            Configs.Main.RankTPColor :
            acc switch
            {
                >= 100f => Configs.Main.RankAPColor,
                >= 95f => Configs.Main.RankSSColor,
                >= 90f => Configs.Main.RankSColor,
                >= 80f => Configs.Main.RankAColor,
                >= 70f => Configs.Main.RankBColor,
                >= 60f => Configs.Main.RankCColor,
                _ => Configs.Main.RankDColor
            };
        return $"{acc:F2}%".Colored(color);
    }

    public static string FormatAccuracyGap()
    {
        var gap = GetCalculatedAccuracy(1) - _history.Accuracy;
        if (Math.Abs(gap) < Precision) return string.Empty;

        var (color, prefix) = gap > 0 ? (Configs.Main.AccuracyGapAheadColor, "+") : (Configs.Main.AccuracyGapBehindColor, "");

        var str = $"{prefix}{gap:F2}%".Colored(color);

        return str.Replace(".00%", "%");
    }

    public static string FormatScoreGap()
    {
        var gap = _current.Score - _history.Score;
        if (gap == 0) return string.Empty;

        var (color, prefix) = gap > 0 ? (Configs.Main.ScoreGapAheadColor, "+") : (Configs.Main.ScoreGapBehindColor, "");

        return Math.Abs(gap) < 1000 ? $"{prefix}{gap}".Colored(color) : $"{prefix}{gap / 1000}K".Colored(color);
    }

    public static string FormatStats()
    {
        var parts = new List<string>();

        if (IsAllPerfect)
        {
            if (IsTruePerfect || !Configs.Main.ShowEarlyLateCounts)
                return string.Empty;
        }
        else
        {
            if (_current.Great > 0)
                parts.Add($"{_current.Great}G".Colored(Configs.Main.GreatCountsColor));
            if (MissCountHittable + _miss.Block > 0)
                parts.Add($"{MissCountHittable + _miss.Block}M".Colored(Configs.Main.NormalMissCountsColor));
            if (GameUtils.BattleUIType != BattleUIItem.Spell && MissCountCollectible > 0)
                parts.Add($"{MissCountCollectible}H".Colored(Configs.Main.CollectibleMissCountsColor));
            if (Configs.Main.EarlyLateCountsDisplayMode != 2)
                return string.Join(" ", parts);
        }

        if (_current.Early > 0)
            parts.Add($"{_current.Early}E".Colored(Configs.Main.EarlyCountsColor));
        if (_current.Late > 0)
            parts.Add($"{_current.Late}L".Colored(Configs.Main.LateCountsColor));
        return string.Join(" ", parts);
    }

    public static string FormatPersonalBestStats()
    {
        if (_history.IsTruePerfect)
            return Configs.Main.TextTruePerfect.Colored(Configs.Main.RankTPColor);

        var parts = new List<string>();

        if (_history.IsAllPerfect)
        {
            if (_history.Early > 0)
                parts.Add($"{_history.Early}E".Colored(Configs.Main.EarlyCountsColor));
            if (_history.Late > 0)
                parts.Add($"{_history.Late}L".Colored(Configs.Main.LateCountsColor));
        }
        else
        {
            if (_history.Great > 0)
                parts.Add($"{_history.Great}G".Colored(Configs.Main.GreatCountsColor));
            if (_history.MissOther > 0)
                parts.Add($"{_history.MissOther}M".Colored(Configs.Main.NormalMissCountsColor));
            if (_history.MissCollectible > 0)
                parts.Add($"{_history.MissCollectible}H".Colored(Configs.Main.CollectibleMissCountsColor));
        }
        return string.Join(" ", parts);
    }

    public static string FormatPersonalBestStatsGap()
    {
        if (_history.IsTruePerfect && IsTruePerfect)
            return string.Empty;

        var parts = new List<string>();

        if (_history.IsAllPerfect && IsAllPerfect)
        {
            parts.Add(FormatSingleStatsGap(_current.Early - _history.Early, "E"));
            parts.Add(FormatSingleStatsGap(_current.Late - _history.Late, "L"));
        }
        else
        {
            parts.Add(FormatSingleStatsGap(_current.Great - _history.Great, "G"));
            parts.Add(FormatSingleStatsGap(MissCountHittable + _miss.Block - _history.MissOther, "M"));
            parts.Add(FormatSingleStatsGap(MissCountCollectible - _history.MissCollectible, "H"));
        }
        return string.Join(" ", parts);
    }

    private static string FormatSingleStatsGap(int gap, string text)
    {
        return gap switch
        {
            < 0 => $"{gap}{text}".Colored(Configs.Main.StatsGapLowerColor),
            > 0 => $"+{gap}{text}".Colored(Configs.Main.StatsGapHigherColor),
            _ => string.Empty
        };
    }

    public static void CountNote(short oid, CountNoteAction action, short doubleOid = -1, bool isLongStart = false)
    {
        switch (action)
        {
            case CountNoteAction.Block:
                if (PlayedNoteIds.Add(oid))
                    _current.Block++;
                break;

            case CountNoteAction.MissMonster:
                if (doubleOid == -1)
                {
                    if (MissedNoteIds.Add(oid))
                        _miss.Monster++;
                }
                else
                {
                    if (MissedNoteIds.Add(oid) && MissedNoteIds.Add(doubleOid))
                        _miss.Monster += 2;
                }
                break;

            case CountNoteAction.MissBlock:
                if (MissedNoteIds.Add(oid))
                    _miss.Block++;
                if (!PlayedNoteIds.Add(oid))
                    _current.Block--;
                break;

            case CountNoteAction.MissLong:
                if (MissedNoteIds.Add(oid))
                {
                    _miss.Long++;
                    if (isLongStart) _miss.LongPair++;
                }
                break;

            case CountNoteAction.MissGhost:
                if (MissedNoteIds.Add(oid))
                    _miss.Ghost++;
                break;

            case CountNoteAction.MissEnergy:
                if (MissedNoteIds.Add(oid))
                    _miss.Energy++;
                break;

            case CountNoteAction.MissMusic:
                if (MissedNoteIds.Add(oid))
                    _miss.Music++;
                break;

            case CountNoteAction.MissMul:
                if (MissedNoteIds.Add(oid))
                {
                    ResetMashing(oid);
                    _miss.Mul++;
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public static void ResetMashing(short oid = -1)
    {
        if (oid < 0)
        {
            MashingNotes.Clear();
            MasherGreatRecords.Clear();
        }
        else
        {
            if (!MissedNoteIds.Contains(oid))
                PlayedNoteIds.Add(oid);
            MashingNotes.Remove(oid);
        }
    }

    public static void CheckMashing()
    {
        foreach (var kvp in MashingNotes)
        {
            var (oid, (note, mashedNum)) = kvp;

            if (MasherGreatRecords[oid] != _current.Great)
            {
                PlayedNoteIds.Add(oid);
                ResetMashing(oid);
                return;
            }

            if (_stage.realTimeTick <= (note.tick + note.configData.length) * 1000)
                continue;

            var tooLow = mashedNum < note.GetMulHitMidThreshold();
            if (tooLow && !MissedNoteIds.Contains(oid) && !PlayedNoteIds.Contains(oid))
            {
                MissedNoteIds.Add(oid);
                _miss.Mul++;
            }

            ResetMashing(oid);
        }
    }

    public static void Mashing(MusicData note)
    {
        var oid = note.objId;
        if (MissedNoteIds.Contains(oid) || PlayedNoteIds.Contains(oid))
            return;

        MasherGreatRecords.TryAdd(oid, _current.Great);
        MashingNotes.TryAdd(oid, (note, 0));
        MashingNotes[oid] = (MashingNotes[oid].Item1, MashingNotes[oid].Item2 + 1);

        CheckMashing();
    }

    public static MusicData GetMusicDataByIdx(int idx)
        => _stage.GetMusicDataByIdx(idx);

    public static MusicData GetCurMusicData()
        => _stage.GetCurMusicData();

    public static void StoreHighestAccuracy(float acc, bool force = false)
        => StoredHighestAccuracy = force ? acc : Math.Max(StoredHighestAccuracy, acc);

    public static void StoreHighestScore(int score, bool force = false)
        => StoredHighestScore = force ? score : Math.Max(StoredHighestScore, score);

    public static void StoreHighestAccuracyFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        IsFirstTry = text == "-";

        if (float.TryParse(text.TrimEnd(' ', '%'), out var x) && x > 0)
            StoreHighestAccuracy(x, true);
        else
            StoredHighestAccuracy = 0;
    }

    public static void StoreHighestScoreFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        IsFirstTry = text == "-";

        if (int.TryParse(text, out var x) && x >= 1)
            StoreHighestScore(x, true);
        else
            StoredHighestScore = 0;
    }

    public static void Init()
    {
        _stage = StageBattleComponent.instance;
        _task = TaskStageTarget.instance;
        _role = BattleRoleAttributeComponent.instance;

        _history.Score = Math.Max(BattleHelper.GetCurrentMusicHighScore(), StoredHighestScore);
        _history.Accuracy = StoredHighestAccuracy;

        Melon<MDIPMod>.Logger.Msg($"Playing: {GameUtils.MusicName}");
        Melon<MDIPMod>.Logger.Msg($"Hash: {GameUtils.MusicHash}");

        var stats = StatsSaverManager.GetStats(GameUtils.MusicHash);
        if (stats != null)
        {
            _history.HasStats = true;
            _history.IsAllPerfect = stats.MissOther + stats.MissCollectible <= 0;
            _history.IsTruePerfect = _history.IsAllPerfect && stats.Early + stats.Late <= 0;
            _history.Great = stats.Great;
            _history.MissOther = stats.MissOther;
            _history.MissCollectible = stats.MissCollectible;
            _history.Early = stats.Early;
            _history.Late = stats.Late;
        }
        else _history.HasStats = false;

        try
        {
            foreach (var note in _stage.GetMusicData())
            {
                var type = (NoteType)note.noteData.type;

                if (!note.isLongPressing)
                {
                    if (note.noteData.addCombo) _total.Hittable++;
                    if (type.IsRegularNote())
                    {
                        _total.Notes++;

                        if (note.isAir)
                        {
                            if (CurrentSkySpeed == -1)
                                CurrentSkySpeed = note.noteData.speed;
                        }
                        else
                        {
                            if (CurrentGroundSpeed == -1)
                                CurrentGroundSpeed = note.noteData.speed;
                        }
                    }
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
        }
        catch (Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }

    public static void Reset(bool includeStoredData = false)
    {
        _stage = null;
        _task = null;

        _current = default;
        _total = default;
        _miss = default;
        _history = default;

        ResetMashing();

        if (includeStoredData)
        {
            IsFirstTry = true;
            StoredHighestAccuracy = 0;
            StoredHighestScore = 0;
        }

        CurrentSkySpeed = -1;
        CurrentGroundSpeed = -1;
        PlayedNoteIds.Clear();
        MissedNoteIds.Clear();
    }
}