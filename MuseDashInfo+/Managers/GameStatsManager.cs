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

    private static Dictionary<int, (MusicData, int)> MashingNotes { get; } = new();
    private static Dictionary<int, int> MasherGreatRecords { get; } = new();
    private static HashSet<int> PlayedNoteIds { get; } = [];
    private static HashSet<int> MissedNoteIds { get; } = [];
    public static int CurrentSkySpeed { get; set; }
    public static int CurrentGroundSpeed { get; set; }

    public static float StoredHighestAccuracy { get; set; }
    public static int StoredHighestScore { get; set; }

    public static CurrentStats Current => _current;
    public static TotalStats Total => _total;
    public static MissStats Miss => _miss;
    public static HistoryStats History => _history;

    // Calculated properties
    public static int MissCountHittable => _miss.Monster + _miss.Long + _miss.Ghost + _miss.Mul;
    public static int MissCountCollectable => _miss.Energy + _miss.Music + _miss.RedPoint;
    public static int MissCount => MissCountHittable + MissCountCollectable + _miss.Block;

    public static float AccuracyTotal => _total.Music + _total.Energy + _total.Hittable + _total.Block;

    public static float AccuracyCounted => _current.Perfect + _current.Great / 2f +
                                           _current.Block + _current.Music + _current.Energy + _current.RedPoint;

    public static float AccuracyRest => Math.Max(0, GetAccuracyRest());

    public static bool IsAvailable => _stage != null && _task != null && _role != null;
    public static bool IsAllPerfect => IsInGame && _current.Great + MissCount < 1;
    public static bool IsTruePerfect => IsAllPerfect && _current.Early + _current.Late < 1;

    public static float GetAccuracyRest() => AccuracyTotal - _current.Perfect - _current.Great -
                                             _current.Block - _current.Music - _current.Energy - _current.RedPoint -
                                             _miss.Music - _miss.Energy - MissCountHittable - _miss.LongPair - _miss.Block;

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

        if (!Configs.Advanced.OutputAccuracyCalculationData)
            return;

        Melon<MDIPMod>.Logger.Warning("=========== Accuracy Stats ===========");
        Melon<MDIPMod>.Logger.Msg($"Total:{AccuracyTotal} | Counted:{AccuracyCounted} | Rest:{AccuracyRest}");
        Melon<MDIPMod>.Logger.Msg($"Total => Music:{Total.Music} | Energy:{Total.Energy} | Block:{Total.Block} | Hittable:{Total.Hittable}");
        Melon<MDIPMod>.Logger.Msg($"Counted => Music:{Current.Music} | Energy:{Current.Energy} | Block:{Current.Block} Perfect:{Current.Perfect} | Great:{Current.Great} /2f | | RedPoint:{Current.RedPoint}");
        Melon<MDIPMod>.Logger.Msg($"Miss => Music:{Miss.Music} | Energy:{Miss.Energy} | Block:{Miss.Block} | Hittable:{MissCountHittable} | LongPair:{Miss.LongPair}");
        Melon<MDIPMod>.Logger.Msg($"{AccuracyTotal} - {Current.Perfect + Current.Great + Current.Block + Current.Music + Current.Energy + Current.RedPoint} - {Miss.Music + Miss.Energy + MissCountHittable + Miss.LongPair + Miss.Block} = {AccuracyRest}");
        Melon<MDIPMod>.Logger.Warning("======================================");
        Melon<MDIPMod>.Logger.Msg($"Calc Acc: {GetCalculatedAccuracy()} | True Acc:{GetTrueAccuracy()}");
        Melon<MDIPMod>.Logger.Warning("======================================");
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
            ? AccuracyCounted / (AccuracyTotal - AccuracyRest)
            : (AccuracyCounted + AccuracyRest) / AccuracyTotal;

        var rounded = MathF.Round(acc / Precision) * Precision;
        return (acc < rounded && SpecialValues.Contains(rounded)
                ? rounded - Precision
                : rounded
            ) * 100f;
    }

    public static string FormatOverview()
    {
        if (IsTruePerfect) return Constants.TEXT_TRUE_PERFECT.Colored(Constants.COLOR_RANK_TP);
        return IsAllPerfect ? Constants.TEXT_ALL_PERFECT.Colored(Constants.COLOR_RANK_AP) : FormatAccuracy();
    }

    public static string FormatAccuracy()
    {
        if (IsAllPerfect) return "100%".Colored(IsTruePerfect ? Constants.COLOR_RANK_TP : Constants.COLOR_RANK_AP);
        var acc = GetCalculatedAccuracy();
        var color = IsTruePerfect ?
            Constants.COLOR_RANK_TP :
            acc switch
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
            if (!GameUtils.IsSpellMode && MissCountCollectable > 0)
                parts.Add($"{MissCountCollectable}H".Colored(Configs.Main.CollectableMissCountsColor));
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
                parts.Add($"{_history.MissCollectible}H".Colored(Configs.Main.CollectableMissCountsColor));
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
            parts.Add(FormatSingleStatsGap(MissCountCollectable - _history.MissCollectible, "H"));
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
                    ResetMashing(id);
                    _miss.Mul++;
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public static void ResetMashing(int id = -1)
    {
        if (id < 0)
        {
            MashingNotes.Clear();
            MasherGreatRecords.Clear();
        }
        else
            MashingNotes.Remove(id);
    }

    public static void CheckMashing()
    {
        foreach (var kvp in MashingNotes)
        {
            var (id, (note, mashedNum)) = kvp;

            if (MasherGreatRecords[id] != _current.Great)
            {
                PlayedNoteIds.Add(id);
                ResetMashing(id);
                return;
            }

            if (_stage.realTimeTick <= (note.tick + note.configData.length) * 1000)
                continue;

            var tooLow = mashedNum < note.GetMulHitMidThreshold();
            if (tooLow && !MissedNoteIds.Contains(id) && !PlayedNoteIds.Contains(id))
            {
                MissedNoteIds.Add(id);
                _miss.Mul++;
            }

            ResetMashing(id);
        }
    }

    public static void Mashing(MusicData note)
    {
        var id = int.Parse(note.noteData.id);
        if (MissedNoteIds.Contains(id) || PlayedNoteIds.Contains(id))
            return;

        MasherGreatRecords.TryAdd(id, _current.Great);
        MashingNotes.TryAdd(id, (note, 0));
        MashingNotes[id] = (MashingNotes[id].Item1, MashingNotes[id].Item2 + 1);

        CheckMashing();
    }

    public static MusicData GetMusicDataByIdx(int id)
        => _stage.GetMusicDataByIdx(id);

    public static MusicData GetCurMusicData()
        => _stage.GetCurMusicData();

    public static void StoreHighestAccuracy(float acc)
        => StoredHighestAccuracy = Math.Max(StoredHighestAccuracy, acc);

    public static void StoreHighestScore(int score)
        => StoredHighestScore = Math.Max(StoredHighestScore, score);

    public static void StoreHighestAccuracyFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        IsFirstTry = text == "-";

        if (float.TryParse(text.TrimEnd(' ', '%'), out var x) && x > 0)
            StoreHighestAccuracy(x);
        else
            StoredHighestAccuracy = 0;
    }

    public static void StoreHighestScoreFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        IsFirstTry = text == "-";

        if (int.TryParse(text, out var x) && x >= 1)
            StoreHighestScore(x);
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

        var stats = StatsSaverManager.GetStats(GlobalDataBase.s_DbMusicTag.CurMusicInfo().uid);
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