using MDIP.Core.Domain.Records;

namespace MDIP.Application.Services.Global.RuntimeData;

public interface IRuntimeDataStore
{
    bool GetOrSetDesiredUiVisible();
    bool GetOrSetDesiredUiVisible(bool desiredUiVisible);
    bool IsFirstTry(string songHash);
    SongRuntimeRecord TryGet(string songHash);
    void AddOrUpdate(string songHash, SongRuntimeRecord songRecord);
    bool StorePersonalBestAccuracyFromText(string songHash, string text);
    bool StorePersonalBestScoreFromText(string songHash, string text);
    bool StorePersonalBestAccuracy(string songHash, float acc);
    bool StorePersonalBestScore(string songHash, int score);
}