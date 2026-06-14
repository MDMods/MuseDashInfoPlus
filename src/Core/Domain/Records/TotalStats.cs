namespace MDIP.Core.Domain.Records;

// Only the totals the accuracy formula reads are tracked. (The game gives no reliable miss-source
// breakdown, so per-type note totals beyond these were write-only and were removed.)
public record struct TotalStats(
    int Hittable,
    int Block,
    int Energy,
    int Music
);