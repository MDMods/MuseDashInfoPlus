using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Nice.Interface;

namespace MDIP.Ecosystem;

// The one place that reads a chart's personal best, always from the ecosystem's OWN authoritative
// store — never scraped from UI text. Score + accuracy(0..100) by music uid + 1-based difficulty.
// `false` means no record exists (a first play).
//
//   - Euterpe : its own save via the pinned cross-mod bridge (EuterpeBridgeClient).
//   - CustomAlbums : its own save via its public API (CamSaveClient; reflected, isolated).
//   - vanilla : the game's own high-score store (DataHelper.highest) — the exact source PnlRecord
//     renders on the prep screen, so we read the source, not the rendered text.
//
// The great/miss/early/late breakdown is deliberately NOT here: no ecosystem persists it (it exists
// only live on the results screen), so Info+ keeps that in its own StatsStore. PB summary = ecosystem
// truth; breakdown = Info+'s own record.
internal static class ChartSource
{
    // CustomAlbums' fixed album id is 999 (AlbumManager.Uid); every CAM chart uid is "999-{index}".
    private const string CustomAlbumsUidPrefix = "999-";

    public static bool TryGetPersonalBest(MusicInfo info, int difficulty, out int score, out float accuracyPercent)
    {
        score = 0;
        accuracyPercent = 0f;
        if (info == null)
            return false;

        var uid = info.uid;

        if (EuterpeBridgeClient.IsEuterpeChart(uid))
            return EuterpeBridgeClient.TryGetPersonalBest(uid, difficulty, out score, out accuracyPercent);

        if (!string.IsNullOrEmpty(uid) && uid.StartsWith(CustomAlbumsUidPrefix, StringComparison.Ordinal))
            return CamSaveClient.TryGetPersonalBest(uid, difficulty, out score, out accuracyPercent);

        return TryGetVanillaPersonalBest(uid, difficulty, out score, out accuracyPercent);
    }

    private static bool TryGetVanillaPersonalBest(string uid, int difficulty, out int score, out float accuracyPercent)
    {
        score = 0;
        accuracyPercent = 0f;
        if (string.IsNullOrEmpty(uid) || DataHelper.highest == null)
            return false;

        var key = $"{uid}_{difficulty}";
        foreach (var data in DataHelper.highest)
        {
            if (data.fields["uid"].GetResult<string>() != key)
                continue;

            score = data.fields["score"].GetResult<int>();
            accuracyPercent = data.fields["accuracy"].GetResult<float>() * 100f; // the game stores 0..1
            return true;
        }

        return false;
    }
}
