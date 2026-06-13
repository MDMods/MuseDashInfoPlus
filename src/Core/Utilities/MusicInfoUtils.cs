using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Enums;

namespace MDIP.Core.Utilities;

public static class MusicInfoUtils
{
    public static BattleUIItem BattleUIType { get; set; }

    public static MusicInfo CurMusicInfo => GlobalDataBase.s_DbBattleStage.selectedMusicInfo;
    public static string CurMusicName => CurMusicInfo.name;
    public static int CurMusicDiff => GlobalDataBase.s_DbBattleStage.selectedDifficulty; // 1-5
    public static string CurMusicDiffStr => CurMusicInfo.GetMusicLevelStringByDiff(CurMusicDiff); // 1-12, and custom text possible
    public static string CurMusicAuthor => CurMusicInfo.author;
    public static string CurMusicLevelDesigner => CurMusicInfo.levelDesigner;
    public static string CurMusicBpm => CurMusicInfo.bpm;
    // Album title / elfin name / character name read from the game's loaded (localized) config via its
    // typed accessors — not by indexing the opaque localization JSON by position + field name, which
    // assumed the array order and key names of an undocumented format. The typed objects ARE the data
    // the game itself displays, so the resolved strings (and their localization) match.
    public static string CurAlbumName
    {
        get
        {
            var album = Singleton<ConfigManager>.instance
                .GetConfigObject<DBConfigAlbums>()
                ?.GetAlbumsInfoByUid(DataHelper.selectedAlbumUid);
            return album?.title ?? string.Empty;
        }
    }

    public static string CurElfin
    {
        get
        {
            var elfinIndex = DataHelper.selectedElfinIndex;
            if (elfinIndex < 0) return string.Empty;

            var info = Singleton<ConfigManager>.instance
                .GetConfigObject<DBConfigElfin>()
                ?.GetElfinInfoByIndex(elfinIndex);
            return info?.name ?? string.Empty;
        }
    }

    public static string CurGirl
    {
        get
        {
            var characterIndex = DataHelper.selectedRoleIndex;
            if (characterIndex < 0) return string.Empty;

            var info = Singleton<ConfigManager>.instance
                .GetConfigObject<DBConfigCharacter>()
                ?.GetCharacterInfoByIndex(characterIndex);
            if (info == null) return string.Empty;

            // "Special" characters carry their display name in characterName; cosmetics use cosName.
            return string.Equals(info.characterType, "Special") ? info.characterName : info.cosName;
        }
    }

    public static string CurMusicHash => (CurMusicDiff + CurMusicAuthor + CurMusicLevelDesigner + CurMusicInfo.musicName).GetConsistentHash();

    public static string GetDifficultyLabel(MainConfigs config)
    {
        return (CurMusicDiff switch
        {
            1 => config.TextDiff1,
            2 => config.TextDiff2,
            3 => config.TextDiff3,
            4 => config.TextDiff4,
            5 => config.TextDiff5,
            _ => "Unknown"
        }).ToUpperInvariant();
    }
}