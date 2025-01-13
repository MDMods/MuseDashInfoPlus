using MelonLoader;
using MuseDashInfoPlus.Manager;
using MuseDashInfoPlus.Utils;

namespace MuseDashInfoPlus;

public class InfoPlusMod : MelonMod
{
    private static MelonPreferences_Category _category;
    // Upper right corner
    private static MelonPreferences_Entry<bool> _displaySongName;
    private static MelonPreferences_Entry<bool> _displaySongDifficulty;
    private static MelonPreferences_Entry<string> _customSongDifficultyFormat;
    // Upper left corner
    private static MelonPreferences_Entry<bool> _displayHitCounts;
    private static MelonPreferences_Entry<string> _customHitCountsFormat;
    private static MelonPreferences_Entry<bool> _displayMissCounts;
    private static MelonPreferences_Entry<string> _customMissCountsFormat;
    private static MelonPreferences_Entry<bool> _displayAccuracy;
    private static MelonPreferences_Entry<bool> _displayScoreGap;
    private static MelonPreferences_Entry<bool> _displayHighestScore;
    private static MelonPreferences_Entry<string> _advancedTextFormat;

    // Upper right corner
    public static bool DisplaySongName => _displaySongName.Value;
    public static bool DisplaySongDifficulty => _displaySongDifficulty.Value;
    public static string CustomSongDifficultyFormat => string.IsNullOrEmpty(_customSongDifficultyFormat?.Value) ? "{diff} - Level {level}" : _customSongDifficultyFormat.Value;
    // Upper left corner
    public static bool DisplayHitCounts => _displayHitCounts.Value;
    public static string CustomHitCountsFormat => string.IsNullOrEmpty(_customHitCountsFormat?.Value) ? "{total} / {hit}" : _customHitCountsFormat.Value;
    public static bool DisplayMissCounts => _displayMissCounts.Value;
    public static bool DisplayAccuracy => _displayAccuracy.Value;
    public static bool DisplayScoreGap => _displayScoreGap.Value;
    public static bool DisplayHighestScore => _displayHighestScore.Value;
    public static string AdvancedTextFormat => string.IsNullOrEmpty(_advancedTextFormat?.Value) ? string.Empty : _advancedTextFormat.Value;

    public static string FinalSongDifficultyTextFormat { get; private set; }
    public static string FinalCountsTextFormat { get; private set; }

    public override void OnInitializeMelon()
    {
        _category = MelonPreferences.CreateCategory("KPD.InfoPlus", "MuseDashInfo+");

        _displaySongName = _category.CreateEntry("DisplaySongName", true, "Display the song name");
        _displaySongDifficulty = _category.CreateEntry("DisplaySongDifficulty", true, "Display the song difficulty");
        _customSongDifficultyFormat = _category.CreateEntry("CustomSongDifficultyFormat", CustomSongDifficultyFormat, "Custom song difficulty text format");

        _displayHitCounts = _category.CreateEntry("DisplayHitCounts", true, "Display hit counts");
        _customHitCountsFormat = _category.CreateEntry("CustomHitCountsFormat", CustomHitCountsFormat, "Custom hit counts text format");
        _displayMissCounts = _category.CreateEntry("DisplayMissCounts", true, "Display miss counts");
        _displayAccuracy = _category.CreateEntry("DisplayAccuracy", true, "Display current accuracy");
        _displayScoreGap = _category.CreateEntry("DisplayScoreGap", true, "Display score gap to highest score");
        _displayHighestScore = _category.CreateEntry("DisplayHighestScore", false, "Display highest score");
        _advancedTextFormat = _category.CreateEntry("AdvancedTextFormat", AdvancedTextFormat, "Advanced text format. Filling in this config will invalidate the formats of other counts and take this config as the standard");

        _category.LoadFromFile(false);
        _category.SaveToFile(false);

        FinalSongDifficultyTextFormat = CustomSongDifficultyFormat;
        string format = string.Empty;
        if (!string.IsNullOrEmpty(AdvancedTextFormat))
        {
            format = AdvancedTextFormat;
        }
        else
        {
            if (DisplayHitCounts) format += CustomHitCountsFormat + " ";
            if (DisplayHighestScore) format += "{highest} ";
            if (DisplayScoreGap) format += "{gap}";
            format = format.TrimEnd();

            if (!string.IsNullOrWhiteSpace(format)) format += "\n";
            if (DisplayMissCounts) format += "{miss}";
            if (DisplayAccuracy) format += "{acc}";
            format = format.TrimEnd();

            if (format.Contains('\n'))
            {
                var lines = format.Split('\n');
                format = $"<size={Constants.COUNTS_SECONDARY_SIZE}>{lines[0]}</size>\n{lines[1]}";
            }
            else if (!string.IsNullOrEmpty(format))
            {
                format = $"<size={Constants.COUNTS_SINGLE_SIZE}>{format}</size>";
            }
        }
        FinalCountsTextFormat = format;
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "GameMain")
        {
            GameStatsUtils.Reload();
        }
        else
        {
            GameStatsUtils.Reset();
            CountsTextManager.Reset();
            UITextUtils.UnloadFonts(TextFontType.SnapsTaste);
        }
    }
}