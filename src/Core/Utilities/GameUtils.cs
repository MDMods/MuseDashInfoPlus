using Il2CppAssets.Scripts.Database;
using Il2CppFormulaBase;
using MDIP.Application.Services.Scoped.Stats;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Enums;

namespace MDIP.Core.Utilities;

public static class GameUtils
{
    public static BattleUIItem BattleUIType { get; set; }

    public static MusicInfo MusicInfo => GlobalDataBase.s_DbMusicTag.m_CurSelectedMusicInfo;
    public static string MusicName => MusicInfo.name;
    public static int MusicDiff => GlobalDataBase.s_DbBattleStage.selectedDifficulty;
    public static string MusicLevel => MusicInfo.GetMusicLevelStringByDiff(MusicDiff);
    public static string MusicAuthor => MusicInfo.author;
    public static string MusicLevelDesigner => MusicInfo.levelDesigner;

    public static string MusicHash => (MusicDiff + MusicAuthor + MusicLevelDesigner + MusicInfo.musicName).GetConsistentHash();

    public static string GetDifficultyLabel(IGameStatsService statsService, MainConfigs config)
    {
        return (MusicDiff switch
        {
            1 => config.TextDiff1,
            2 => config.TextDiff2,
            3 => config.TextDiff3,
            4 => config.TextDiff4,
            5 => config.TextDiff5,
            _ => "Unknown"
        }).ToUpperInvariant();
    }

    public static void Reset()
        => BattleUIType = BattleUIItem.Unknown;
}