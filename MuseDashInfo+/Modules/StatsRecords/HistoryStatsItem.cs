namespace MDIP.Modules.StatsRecords;

public record struct HistoryStats(
    int Score,
    float Accuracy,
    bool HasStats,
    bool IsAllPerfect,
    bool IsTruePerfect,
    int Great,
    int MissOther,
    int MissCollectible,
    int Early,
    int Late
);