using System.Globalization;
using MDIP.Core.Domain.Records;

namespace MDIP.Application.Services.Global.RuntimeData;

public class RuntimeSongDataStore : IRuntimeSongDataStore
{
    private readonly Dictionary<string, SongRuntimeRecord> _songRuntimeRecords = new();

    public bool IsFirstTry(string songHash)
        => !_songRuntimeRecords.ContainsKey(songHash);

    public SongRuntimeRecord TryGet(string songHash)
        => _songRuntimeRecords.TryGetValue(songHash, out var data) ? data : new(0, 0);

    public void AddOrUpdate(string songHash, SongRuntimeRecord songRecord)
        => _songRuntimeRecords[songHash] = songRecord;

    public bool StorePersonalBestAccuracyFromText(string songHash, string text)
    {
        if (string.IsNullOrEmpty(songHash) || string.IsNullOrEmpty(text))
            return false;

        var trimmed = text.TrimEnd(' ', '%');
        if (TryParseAccuracy(trimmed, out var acc) && acc > 0)
            return StorePersonalBestAccuracy(songHash, acc);

        return false;
    }

    public bool StorePersonalBestScoreFromText(string songHash, string text)
    {
        if (string.IsNullOrEmpty(songHash) || string.IsNullOrEmpty(text))
            return false;

        if (int.TryParse(text, out var score) && score > 0)
            return StorePersonalBestScore(songHash, score);

        return false;
    }

    public bool StorePersonalBestAccuracy(string songHash, float acc)
    {
        if (acc <= 0)
            return false;

        if (_songRuntimeRecords.TryGetValue(songHash, out var record))
        {
            if (record.PersonalBestAccuracy >= acc)
                return false;

            _songRuntimeRecords[songHash] = record with
            {
                PersonalBestAccuracy = acc
            };
            return true;
        }

        _songRuntimeRecords.Add(songHash, new(acc, 0));
        return true;
    }

    public bool StorePersonalBestScore(string songHash, int score)
    {
        if (score <= 0)
            return false;

        if (_songRuntimeRecords.TryGetValue(songHash, out var record))
        {
            if (record.PersonalBestScore >= score)
                return false;

            _songRuntimeRecords[songHash] = record with
            {
                PersonalBestScore = score
            };
            return true;
        }

        _songRuntimeRecords.Add(songHash, new(0, score));
        return true;
    }

    private static bool TryParseAccuracy(string text, out float value)
        => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) || float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
}