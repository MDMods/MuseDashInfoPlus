namespace MDIP.Modules.Stats;

public record struct HistoryStats(
    int Score,
    float Accuracy,
    int Great,
    int MissHittable,
    int MissCollectable,
    int MissBlock,
    int Early,
    int Late
);