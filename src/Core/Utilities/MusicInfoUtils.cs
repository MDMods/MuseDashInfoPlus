using Il2CppAssets.Scripts.Database;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Enums;

namespace MDIP.Core.Utilities;

public static class MusicInfoUtils
{
    public static BattleUIItem BattleUIType { get; set; }

    public static MusicInfo CurMusicInfo => GlobalDataBase.s_DbMusicTag.m_CurSelectedMusicInfo;
    public static string CurMusicName => CurMusicInfo.name;
    public static int CurMusicDiff => GlobalDataBase.s_DbBattleStage.selectedDifficulty;
    public static string CurMusicDiffStr => CurMusicInfo.GetMusicLevelStringByDiff(CurMusicDiff);
    public static string CurMusicAuthor => CurMusicInfo.author;
    public static string CurMusicLevelDesigner => CurMusicInfo.levelDesigner;
    public static string CurMusicBpm => CurMusicInfo.bpm;

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