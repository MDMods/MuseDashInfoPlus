using MDIP.Utils;

namespace MDIP.Managers;

public static class TextDataManager
{
    public static string ChartDifficultyTextFormat { get; private set; }
    public static string GameStatsTextFormat { get; private set; }
    public static string ScoreStatsTextFormat { get; private set; }
    public static string NoteStatsTextFormat { get; private set; }

    public static void ConstractTextFormats()
    {
        string Separator = $"<size={Constants.SEPARATOR_SIZE}><color={Constants.SEPARATOR_COLOR}>{Configs.Main.CustomSeparator}</color></size>";

        ChartDifficultyTextFormat = Configs.Main.CustomChartDifficultyFormat;

        string format = string.Empty;
        if (Configs.Main.DisplayAccuracy) format += "{acc}";
        if (Configs.Main.DisplayMissCounts) format += (Configs.Main.DisplayAccuracy ? Separator : string.Empty) + "{miss}";
        GameStatsTextFormat = format.Trim();

        format = string.Empty;
        if (Configs.Main.DisplayHighestScore) format += "{highest}";
        if (Configs.Main.DisplayScoreGap) format += (Configs.Main.DisplayHighestScore ? Separator : string.Empty) + "{gap}";
        ScoreStatsTextFormat = format.Trim();

        NoteStatsTextFormat = Configs.Main.CustomNoteCountsFormat;
    }
}
