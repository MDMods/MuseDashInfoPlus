using Il2CppAssets.Scripts.Database;
using Il2CppFormulaBase;
using MDIP.Modules.Enums;

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

    public static string MusicDiffStr => (MusicDiff switch
    {
        1 => Configs.Main.TextDiff1,
        2 => Configs.Main.TextDiff2,
        3 => Configs.Main.TextDiff3,
        4 => Configs.Main.TextDiff4,
        5 => Configs.Main.TextDiff5,
        _ => "Unknown"
    }).ToUpper();

    public static void Reset()
        => BattleUIType = BattleUIItem.Unknown;
}