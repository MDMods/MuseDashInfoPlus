using Il2CppAssets.Scripts.Database;
using Il2CppFormulaBase;
using MDIP.Application.Contracts;
using MDIP.Domain.Configs;
using MDIP.Domain.Enums;

namespace MDIP.Utils;

public static class GameUtils
{
    public static BattleUIItem BattleUIType { get; set; }

    public static MusicInfo MusicInfo => GlobalDataBase.s_DbBattleStage.selectedMusicInfo;
    public static string MusicName => GlobalDataBase.s_DbBattleStage.selectedMusicName;
    public static int MusicDiff => GlobalDataBase.s_DbBattleStage.selectedDifficulty;
    public static string MusicLevel => MusicInfo.GetMusicLevelStringByDiff(MusicDiff);
    public static string MusicAuthor => MusicInfo.author;

    public static string MusicHash => (GlobalDataBase.s_DbBattleStage.selectedMusicFileName
                                       + MusicInfo.levelDesigner
                                       + StageBattleComponent.instance.GetMusicData().Count
                                       + MusicDiff).GetConsistentHash();

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