using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using JetBrains.Annotations;
using MDIP.Application.Contracts;
using MDIP.Application.Services.Notes;
using MDIP.Domain.Configs;
using MDIP.Domain.Enums;
using MDIP.Domain.Records;
using MDIP.Utils;

namespace MDIP.Application.Services.Stats;

public class GameStatsService : IGameStatsService
{
    private const float Precision = 0.0001f;

    private readonly HashSet<short> _missedNoteIds = [];
    private readonly HashSet<short> _playedNoteIds = [];
    private readonly HashSet<float> _specialValues = [0.6f, 0.7f, 0.8f, 0.9f, 1f];

    private CurrentStats _current;
    private HistoryStats _history;
    private MissStats _miss;

    private BattleRoleAttributeComponent _role;
    private StageBattleComponent _stage;
    private TaskStageTarget _task;
    private TotalStats _total;

    public bool IsInGame { get; set; }
    public bool IsFirstTry { get; set; } = true;
    public CurrentStats Current => _current;
    public TotalStats Total => _total;
    public MissStats Miss => _miss;
    public HistoryStats History => _history;
    public int CurrentSkySpeed { get; set; } = -1;
    public int CurrentGroundSpeed { get; set; } = -1;
    public float StoredHighestAccuracy { get; set; }
    public int StoredHighestScore { get; set; }

    public int MissCountHittable => _miss.Monster + _miss.Long + _miss.Mul;
    public int MissCountCollectible => _miss.Energy + _miss.Music + _miss.RedPoint + _miss.Ghost;
    public int MissCount => MissCountHittable + MissCountCollectible + _miss.Block;
    public float AccuracyCalculationTotal => _total.Music + _total.Energy + _total.Hittable + _total.Block;
    public float AccuracyCalculationCounted => _current.Perfect + _current.Great / 2f + _current.Block + _current.Music + _current.Energy + _current.RedPoint;
    public float AccuracyCalculationRest => Math.Max(0, GetAccuracyRest());
    public bool IsAvailable => _stage != null && _task != null && _role != null;
    public bool IsAllPerfect => IsInGame && _current.Great + MissCount < 1;
    public bool IsTruePerfect => IsAllPerfect && _current.Early + _current.Late < 1;

    public float GetAccuracyRest() => AccuracyCalculationTotal - _current.Perfect - _current.Great - _current.Block - _current.Music - _current.Energy - _current.RedPoint - MissCount - _miss.LongPair;

    public void UpdateCurrentStats()
    {
        if (!IsAvailable)
            return;

        _current = new(
            _task?.m_PerfectResult ?? 0,
            _task?.m_GreatResult ?? 0,
            _role?.early ?? 0,
            _role?.late ?? 0,
            _task?.m_MusicCount ?? 0,
            _task?.m_EnergyCount ?? 0,
            _current.Block,
            _task?.m_RedPoint ?? 0,
            _task?.m_Score ?? 0
        );

        if (ConfigAccessor.Advanced.DisplayNoteDebuggingData)
            OutputCurrentNoteDebuggingData(true);
    }

    public void OutputCurrentNoteDebuggingData(bool isInGame)
    {
        var advanced = ConfigAccessor.Advanced;
        if (!advanced.DisplayNoteDebuggingData)
            return;

        if (isInGame)
            Logger.Warning($"=========== Tick: {_stage?.realTimeTick ?? -1} | CurId: {_stage?.curTimeNodeOrder?.idx ?? -1} ===========");
        else
            Logger.Error("=========== Note Count Error ===========");
        Logger.Info($"Total:{AccuracyCalculationTotal} | Counted:{AccuracyCalculationCounted} | Rest:{AccuracyCalculationRest}");
        Logger.Info($"Total => Music:{Total.Music} | Energy:{Total.Energy} | Block:{Total.Block} | RedPoint:{Total.RedPoint} | Hittable:{Total.Hittable}");
        Logger.Info($"Counted => Music:{Current.Music} | Energy:{Current.Energy} | Block:{Current.Block} | RedPoint:{Current.RedPoint} | Perfect:{Current.Perfect} | Great:{Current.Great} /2f");
        Logger.Info($"Miss => Music:{Miss.Music} | Energy:{Miss.Energy} | Block:{Miss.Block} | RedPoint:{Miss.RedPoint} | Hittable:{MissCountHittable} | LongPair:{Miss.LongPair} | Ghost:{Miss.Ghost}");
        Logger.Info($"{AccuracyCalculationTotal} - {Current.Perfect + Current.Great + Current.Block + Current.Music + Current.Energy + Current.RedPoint} - {Miss.Music + Miss.Energy + MissCountHittable + Miss.LongPair + Miss.Block + Miss.RedPoint + Miss.Ghost} = {AccuracyCalculationRest}");
        if (!isInGame)
            Logger.Error("======================================");
        Logger.Info($"Calc Acc: {GetCalculatedAccuracy()} | True Acc:{GetTrueAccuracy()}");
        if (!isInGame)
            Logger.Error("======================================");
    }

    public void UpdateCurrentSpeed(bool isAir, int speed)
    {
        if (isAir)
            CurrentSkySpeed = speed;
        else
            CurrentGroundSpeed = speed;
    }

    public float GetTrueAccuracy() => _task.GetAccuracy() * 100f;

    public float GetCalculatedAccuracy(int mode = -1)
    {
        var main = ConfigAccessor.Main;
        if (mode < 1)
            mode = main.AccuracyDisplayMode;

        var acc = mode == 2
            ? AccuracyCalculationCounted / (AccuracyCalculationTotal - AccuracyCalculationRest)
            : (AccuracyCalculationCounted + AccuracyCalculationRest) / AccuracyCalculationTotal;

        var rounded = MathF.Round(acc / Precision) * Precision;
        return (acc < rounded && _specialValues.Contains(rounded) ? rounded - Precision : rounded) * 100f;
    }

    public string FormatOverview()
    {
        var main = ConfigAccessor.Main;
        if (IsTruePerfect)
            return main.TextTruePerfect.Colored(main.RankTPColor);
        return IsAllPerfect ? main.TextAllPerfect.Colored(main.RankAPColor) : FormatAccuracy();
    }

    public string FormatRank()
    {
        var main = ConfigAccessor.Main;
        if (GameUtils.BattleUIType == BattleUIItem.Spell)
            return string.Empty;
        if (IsTruePerfect)
            return main.TextTruePerfect.Colored(main.RankTPColor);
        if (IsAllPerfect)
            return main.TextAllPerfect.Colored(main.RankAPColor);

        var acc = GetCalculatedAccuracy();
        return acc switch
        {
            >= 95f => "SS".Colored(main.RankSSColor),
            >= 90f => "S".Colored(main.RankSColor),
            >= 80f => "A".Colored(main.RankAColor),
            >= 70f => "B".Colored(main.RankBColor),
            >= 60f => "C".Colored(main.RankCColor),
            _ => "D".Colored(main.RankDColor)
        };
    }

    public string FormatAccuracy()
    {
        var main = ConfigAccessor.Main;
        if (GameUtils.BattleUIType == BattleUIItem.Spell)
            return string.Empty;
        if (IsAllPerfect)
            return "100%".Colored(IsTruePerfect ? main.RankTPColor : main.RankAPColor);

        var acc = GetCalculatedAccuracy();
        var color = IsTruePerfect
            ? main.RankTPColor
            : acc switch
            {
                >= 100f => main.RankAPColor,
                >= 95f => main.RankSSColor,
                >= 90f => main.RankSColor,
                >= 80f => main.RankAColor,
                >= 70f => main.RankBColor,
                >= 60f => main.RankCColor,
                _ => main.RankDColor
            };
        return FormattableString.Invariant($"{acc:F2}%").Colored(color);
    }

    public string FormatAccuracyGap()
    {
        var main = ConfigAccessor.Main;
        if (GameUtils.BattleUIType == BattleUIItem.Spell)
            return string.Empty;

        var gap = GetCalculatedAccuracy(1) - _history.Accuracy;
        if (Math.Abs(gap) < Precision)
            return string.Empty;

        var (color, prefix) = gap > 0 ? (main.AccuracyGapAheadColor, "+") : (main.AccuracyGapBehindColor, "");
        var str = FormattableString.Invariant($"{prefix}{gap:F2}%").Colored(color);
        return str.Replace(".00%", "%");
    }

    public string FormatScoreGap()
    {
        var main = ConfigAccessor.Main;
        var gap = _current.Score - _history.Score;
        if (gap == 0)
            return string.Empty;

        var (color, prefix) = gap > 0 ? (main.ScoreGapAheadColor, "+") : (main.ScoreGapBehindColor, "");
        return Math.Abs(gap) < 1000 ? $"{prefix}{gap}".Colored(color) : $"{prefix}{gap / 1000}K".Colored(color);
    }

    public string FormatStats()
    {
        var main = ConfigAccessor.Main;
        var parts = new List<string>();

        if (IsAllPerfect)
        {
            if (IsTruePerfect || !main.ShowEarlyLateCounts)
                return string.Empty;
        }
        else
        {
            if (_current.Great > 0)
                parts.Add($"{_current.Great}G".Colored(main.GreatCountsColor));
            if (MissCountHittable + _miss.Block > 0)
                parts.Add($"{MissCountHittable + _miss.Block}M".Colored(main.NormalMissCountsColor));
            if (GameUtils.BattleUIType != BattleUIItem.Spell && MissCountCollectible > 0)
                parts.Add($"{MissCountCollectible}H".Colored(main.CollectibleMissCountsColor));
            if (main.EarlyLateCountsDisplayMode != 2)
                return string.Join(" ", parts);
        }

        if (_current.Early > 0)
            parts.Add($"{_current.Early}E".Colored(main.EarlyCountsColor));
        if (_current.Late > 0)
            parts.Add($"{_current.Late}L".Colored(main.LateCountsColor));
        return string.Join(" ", parts);
    }

    public string FormatPersonalBestStats()
    {
        var main = ConfigAccessor.Main;
        if (_history.IsTruePerfect)
            return main.TextTruePerfect.Colored(main.RankTPColor);

        var parts = new List<string>();
        if (_history.IsAllPerfect)
        {
            if (_history.Early > 0)
                parts.Add($"{_history.Early}E".Colored(main.EarlyCountsColor));
            if (_history.Late > 0)
                parts.Add($"{_history.Late}L".Colored(main.LateCountsColor));
        }
        else
        {
            if (_history.Great > 0)
                parts.Add($"{_history.Great}G".Colored(main.GreatCountsColor));
            if (_history.MissOther > 0)
                parts.Add($"{_history.MissOther}M".Colored(main.NormalMissCountsColor));
            if (_history.MissCollectible > 0)
                parts.Add($"{_history.MissCollectible}H".Colored(main.CollectibleMissCountsColor));
        }

        return string.Join(" ", parts);
    }

    public string FormatPersonalBestStatsGap()
    {
        var main = ConfigAccessor.Main;
        if (_history.IsTruePerfect && IsTruePerfect)
            return string.Empty;

        var parts = new List<string>();
        if (_history.IsAllPerfect && IsAllPerfect)
        {
            parts.Add(FormatSingleStatsGap(_current.Early - _history.Early, "E", main));
            parts.Add(FormatSingleStatsGap(_current.Late - _history.Late, "L", main));
        }
        else
        {
            parts.Add(FormatSingleStatsGap(_current.Great - _history.Great, "G", main));
            parts.Add(FormatSingleStatsGap(MissCountHittable + _miss.Block - _history.MissOther, "M", main));
            parts.Add(FormatSingleStatsGap(MissCountCollectible - _history.MissCollectible, "H", main));
        }

        return string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    public void CountNote(short oid, CountNoteAction action, short doubleOid = -1, bool isLongStart = false, float time = 0f)
    {
        switch (action)
        {
            case CountNoteAction.Block:
                if (_playedNoteIds.Add(oid))
                    _current.Block++;
                break;

            case CountNoteAction.MissMonster:
                if (doubleOid == -1)
                {
                    if (_missedNoteIds.Add(oid))
                        _miss.Monster++;
                }
                else
                {
                    if (_missedNoteIds.Add(oid) && _missedNoteIds.Add(doubleOid))
                        _miss.Monster += 2;
                }

                break;

            case CountNoteAction.MissBlock:
                if (_missedNoteIds.Add(oid))
                    _miss.Block++;
                if (!_playedNoteIds.Add(oid))
                    _current.Block--;
                break;

            case CountNoteAction.MissLong:
                if (_missedNoteIds.Add(oid))
                {
                    _miss.Long++;
                    if (isLongStart)
                        _miss.LongPair++;
                }
                break;

            case CountNoteAction.MissGhost:
                if (_missedNoteIds.Add(oid))
                    _miss.Ghost++;
                break;

            case CountNoteAction.MissEnergy:
                if (_missedNoteIds.Add(oid))
                    _miss.Energy++;
                break;

            case CountNoteAction.MissMusic:
                if (_missedNoteIds.Add(oid))
                    _miss.Music++;
                break;

            case CountNoteAction.Mul:
                if (_playedNoteIds.Add(oid))
                {
                    if (_missedNoteIds.Remove(oid))
                        _miss.Mul--;
                }
                break;

            case CountNoteAction.MissMul:
                var curTick = _stage.realTimeTick;
                MelonCoroutines.Start(CoroutineUtils.Run(() =>
                {
                    if (_stage.realTimeTick <= curTick)
                        return;
                    if (!_playedNoteIds.Contains(oid) && _missedNoteIds.Add(oid))
                        _miss.Mul++;
                }, time));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public void CountMul(short oid, int result, float time)
    {
        switch (result)
        {
            case 0 or 1:
                CountNote(oid, CountNoteAction.MissMul, time: time);
                break;
            case 3 or 4:
                CountNote(oid, CountNoteAction.Mul);
                break;
            default:
                Logger.Warning($"Unknown result type of mul: {result}");
                break;
        }
    }

    public MusicData GetMusicDataByIdx(int idx) => _stage.GetMusicDataByIdx(idx);
    public MusicData GetCurMusicData() => _stage.GetCurMusicData();

    public void StoreHighestAccuracy(float acc, bool force = false)
    {
        StoredHighestAccuracy = force ? acc : Math.Max(StoredHighestAccuracy, acc);
    }

    public void StoreHighestScore(int score, bool force = false)
    {
        StoredHighestScore = force ? score : Math.Max(StoredHighestScore, score);
    }

    public void StoreHighestAccuracyFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        IsFirstTry = text == "-";

        if (float.TryParse(text.TrimEnd(' ', '%'), out var x) && x > 0)
            StoreHighestAccuracy(x, true);
        else
            StoredHighestAccuracy = 0;
    }

    public void StoreHighestScoreFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        IsFirstTry = text == "-";

        if (int.TryParse(text, out var x) && x >= 1)
            StoreHighestScore(x, true);
        else
            StoredHighestScore = 0;
    }

    public void Init()
    {
        _stage = StageBattleComponent.instance;
        _task = TaskStageTarget.instance;
        _role = BattleRoleAttributeComponent.instance;

        _history.Score = Math.Max(BattleHelper.GetCurrentMusicHighScore(), StoredHighestScore);
        _history.Accuracy = StoredHighestAccuracy;

        Logger.Info($"Playing: {GameUtils.MusicName}");
        Logger.Info($"Hash: {GameUtils.MusicHash}");

        var stats = StatsSaverService.GetStats(GameUtils.MusicHash);
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
        else
            _history.HasStats = false;

        try
        {
            foreach (var note in _stage.GetMusicData())
            {
                var type = (NoteType)note.noteData.type;

                if (!note.isLongPressing)
                {
                    if (note.noteData.addCombo)
                        _total.Hittable++;
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
                    case NoteType.Monster:
                        _total.Monster++;
                        break;
                    case NoteType.Block:
                        _total.Block++;
                        break;
                    case NoteType.Long when !note.isLongPressing:
                        _total.Long++;
                        break;
                    case NoteType.Ghost:
                        _total.Ghost++;
                        break;
                    case NoteType.Boss:
                        _total.Boss++;
                        break;
                    case NoteType.Energy:
                        _total.Energy++;
                        break;
                    case NoteType.Music:
                        _total.Music++;
                        break;
                    case NoteType.Mul:
                        _total.Mul++;
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }
    }

    public void Reset(bool includeStoredData = false)
    {
        _stage = null;
        _task = null;
        _role = null;
        _current = default;
        _total = default;
        _miss = default;
        _history = default;

        if (includeStoredData)
        {
            IsFirstTry = true;
            StoredHighestAccuracy = 0;
            StoredHighestScore = 0;
        }

        CurrentSkySpeed = -1;
        CurrentGroundSpeed = -1;
        _playedNoteIds.Clear();
        _missedNoteIds.Clear();
    }

    private static string FormatSingleStatsGap(int gap, string text, MainConfigs main)
    {
        return gap switch
        {
            < 0 => $"{gap}{text}".Colored(main.StatsGapLowerColor),
            > 0 => $"+{gap}{text}".Colored(main.StatsGapHigherColor),
            _ => string.Empty
        };
    }

    #region Injections

    [UsedImplicitly] public required IConfigAccessor ConfigAccessor {get; init;}
    [UsedImplicitly] public required IStatsSaverService StatsSaverService {get; init;}
    [UsedImplicitly] public required ILogger<GameStatsService> Logger {get; init;}

    #endregion
}