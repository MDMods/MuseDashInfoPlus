using System.Globalization;
using System.Text;

namespace MDIP.Utils;

public static class Configs
{
	public static MainConfigs Main => ConfigManager.GetConfig<MainConfigs>(nameof(MainConfigs));
	public static AdvancedConfigs Advanced => ConfigManager.GetConfig<AdvancedConfigs>(nameof(AdvancedConfigs));

	public static TextFieldLowerLeftConfigs TextFieldLowerLeft => ConfigManager.GetConfig<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs));
	public static TextFieldLowerRightConfigs TextFieldLowerRight => ConfigManager.GetConfig<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs));
	public static TextFieldScoreBelowConfigs TextFieldScoreBelow => ConfigManager.GetConfig<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs));
	public static TextFieldScoreRightConfigs TextFieldScoreRight => ConfigManager.GetConfig<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs));
	public static TextFieldUpperLeftConfigs TextFieldUpperLeft => ConfigManager.GetConfig<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs));
	public static TextFieldUpperRightConfigs TextFieldUpperRight => ConfigManager.GetConfig<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs));

	public static string GetConfigPath(string fileName)
	{
		var configFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "UserData", "Info+"));
		if (!Directory.Exists(configFolder))
		{
			Directory.CreateDirectory(configFolder);
			Melon<MDIPMod>.Logger.Msg("Created config directory");
		}

		var fullPath = Path.GetFullPath(Path.Combine(configFolder, fileName));
		return fullPath;
	}

	public static string GenerateComments(string chineseComment, string englishComment)
	{
		var currentCulture = CultureInfo.CurrentCulture;
		var isChineseCulture = currentCulture.Name.StartsWith("zh");
		var comment = isChineseCulture ? chineseComment : englishComment;

		return string.Join(Environment.NewLine, comment.Split("\n").Select(line => "# " + line));
	}

	public static string AddCommentsToYaml(string yaml, Dictionary<string, (string zh, string en)> comments)
	{
		var result = new StringBuilder();
		var lines = yaml.Split(Environment.NewLine);

		foreach (var line in lines)
		{
			var trimmedLine = line.Trim();
			foreach (var comment in comments.Where(comment => trimmedLine.StartsWith(comment.Key, StringComparison.OrdinalIgnoreCase)))
			{
				result.AppendLine(GenerateComments(comment.Value.zh, comment.Value.en));
				break;
			}

			result.AppendLine(line);
		}

		return result.ToString();
	}
}