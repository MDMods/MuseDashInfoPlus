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
    public static string CurAlbumName => Singleton<ConfigManager>.m_Instance.GetConfigStringValue("albums", "uid", "title", DataHelper.selectedAlbumUid);
    public static string CurElfin {
        get
        {   // Reference: https://github.com/MDMods/CurrentCombination/blob/main/Managers/ModManager.cs
            var elfinIndex = DataHelper.selectedElfinIndex;

            if (elfinIndex < 0) return string.Empty;

            return Singleton<ConfigManager>.instance
                .GetJson("elfin", true)[elfinIndex]
                ["name"].ToString();
        }
    }
    public static string CurGirl {
        get
        {   // Reference: https://github.com/MDMods/CurrentCombination/blob/main/Managers/ModManager.cs
            var characterIndex = DataHelper.selectedRoleIndex;

            if (characterIndex < 0) return string.Empty;

            var configManager = Singleton<ConfigManager>.instance;
            var character = configManager.GetJson("character", true)[characterIndex];

            var characterType = configManager.GetConfigObject<DBConfigCharacter>()
                .GetCharacterInfoByIndex(characterIndex)
                .characterType;

            return string.Equals(characterType, "Special")
                ? character["characterName"].ToString()
                : character["cosName"].ToString();
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