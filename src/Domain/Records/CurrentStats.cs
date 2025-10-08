namespace MDIP.Domain.Records;

public record struct CurrentStats(
    int Perfect,
    int Great,
    int Early,
    int Late,
    int Music,
    int Energy,
    int Block,
    int RedPoint,
    int Score
);