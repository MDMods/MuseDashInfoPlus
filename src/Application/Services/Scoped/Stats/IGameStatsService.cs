using Il2CppGameLogic;
using MDIP.Core.Domain.Enums;
using MDIP.Core.Domain.Records;

namespace MDIP.Application.Services.Scoped.Stats;

public interface IGameStatsService
{
    bool IsPlayerPlaying { get; set; }
    string PlayingMusicHash { get; set; }
    CurrentStats Current { get; }
    TotalStats Total { get; }
    MissStats Miss { get; }
    HistoryStats History { get; }
    int MissCountHittable { get; }
    int MissCountCollectible { get; }
    int MissCount { get; }
    float AccuracyCalculationTotal { get; }
    float AccuracyCalculationCounted { get; }
    float AccuracyCalculationRest { get; }
    int CurrentSkySpeed { get; set; }
    int CurrentGroundSpeed { get; set; }
    bool IsAvailable { get; }
    bool IsAllPerfect { get; }
    bool IsTruePerfect { get; }
    float GetAccuracyRest();
    void UpdateCurrentStats();
    void OutputCurrentNoteDebuggingData(bool isInGame);
    void UpdateCurrentSpeed(bool isAir, int speed);
    float GetTrueAccuracy();
    float GetCalculatedAccuracy(int mode = -1);
    string FormatOverview();
    string FormatRank();
    string FormatAccuracy();
    string FormatAccuracyGap();
    string FormatScoreGap();
    string FormatStats();
    string FormatPersonalBestStats();
    string FormatPersonalBestStatsGap();
    void CountNote(short oid, CountNoteAction action, short doubleOid = -1, bool isLongStart = false, float time = 0f);
    void CountMul(short oid, int result, float time);
    MusicData GetMusicDataByIdx(int idx);
    MusicData GetCurMusicData();
    void Init();
}