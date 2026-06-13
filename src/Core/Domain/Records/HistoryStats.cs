namespace MDIP.Core.Domain.Records;

public record struct HistoryStats(
    int Score,
    float Accuracy,
    bool HasPersonalBest, // a score/accuracy PB exists in the ecosystem's authoritative store
    bool HasStats,        // a great/miss/early/late breakdown exists in Info+'s own StatsStore

    bool IsAllPerfect,
    bool IsTruePerfect,
    int Great,
    int MissOther,
    int MissCollectible,
    int Early,
    int Late
);