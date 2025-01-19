using Il2CppAssets.Scripts.Database;

using MDIP.Managers;

namespace MDIP.Utils;

public static class GameInfosUtils
{
    public static string MusicName => GlobalDataBase.dbBattleStage.selectedMusicName;
    public static int MusicDiff => GlobalDataBase.dbBattleStage.selectedDifficulty;
    public static string MusicLevel => GlobalDataBase.dbBattleStage.selectedMusicInfo.GetMusicLevelStringByDiff(MusicDiff);
    public static string MusicDiffStr => (MusicDiff switch
    {
        1 => "Easy",
        2 => "Hard",
        3 => "Master",
        4 => "Hidden",
        5 => "Special",
        _ => "Unknown"
    }).ToUpper();

    public static string GetChartInfosString()
    {
        var text = string.Empty;
        if (ConfigManager.DisplayChartName) text = $"<b>{MusicName.TruncateByWidth(45).Color(ConfigManager.ChartNameColor)}</b>\n";
        if (ConfigManager.DisplayChartDifficulty) text += $"<size={Constants.CHART_DIFFICULTY_SIZE}>{ConfigManager.FinalChartDifficultyTextFormat.Replace("{diff}", MusicDiffStr).Replace("{level}", MusicLevel)}</size>";
        return text;
    }
}