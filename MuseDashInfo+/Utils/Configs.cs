using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MDIP.Managers;
using MDIP.Modules;
using MDIP.Modules.Configs;
using MelonLoader;
using UnityEngine;

namespace MDIP.Utils;

public static class Configs
{
	public static MainConfigs Main => ConfigManager.Instance.GetConfig<MainConfigs>(ConfigName.MainConfigs);
	public static AdvancedConfigs Advanced => ConfigManager.Instance.GetConfig<AdvancedConfigs>(ConfigName.AdvancedConfigs);

	public static TextFieldLowerLeftConfigs TextFieldLowerLeft => ConfigManager.Instance.GetConfig<TextFieldLowerLeftConfigs>(ConfigName.TextFieldLowerLeftConfigs);
	public static TextFieldLowerRightConfigs TextFieldLowerRight => ConfigManager.Instance.GetConfig<TextFieldLowerRightConfigs>(ConfigName.TextFieldLowerRightConfigs);
	public static TextFieldScoreBelowConfigs TextFieldScoreBelow => ConfigManager.Instance.GetConfig<TextFieldScoreBelowConfigs>(ConfigName.TextFieldScoreBelowConfigs);
	public static TextFieldScoreRightConfigs TextFieldScoreRight => ConfigManager.Instance.GetConfig<TextFieldScoreRightConfigs>(ConfigName.TextFieldScoreRightConfigs);
	public static TextFieldUpperLeftConfigs TextFieldUpperLeft => ConfigManager.Instance.GetConfig<TextFieldUpperLeftConfigs>(ConfigName.TextFieldUpperLeftConfigs);
	public static TextFieldUpperRightConfigs TextFieldUpperRight => ConfigManager.Instance.GetConfig<TextFieldUpperRightConfigs>(ConfigName.TextFieldUpperRightConfigs);

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
		var lines = yaml.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

		foreach (var line in lines)
		{
			var trimmedLine = line.Trim();
			foreach (var comment in comments)
			{
				if (trimmedLine.StartsWith(comment.Key, StringComparison.OrdinalIgnoreCase))
				{
					result.AppendLine(GenerateComments(comment.Value.zh, comment.Value.en));
					break;
				}
			}

			result.AppendLine(line);
		}

		return result.ToString();
	}
}