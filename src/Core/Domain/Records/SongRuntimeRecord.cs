namespace MDIP.Core.Domain.Records;

public record struct SongRuntimeRecord(
    float PersonalBestAccuracy,
    int PersonalBestScore
);