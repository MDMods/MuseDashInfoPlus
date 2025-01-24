using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using MDIP.Managers;
using MDIP.Modules.Configs;
using MDIP.Modules;

namespace MDIP.Utils;

public static class Configs
{
    public static MainConfigs Main => ConfigManager.Instance.GetConfig<MainConfigs>(ConfigName.MainConfigs);
    public static AdvancedConfigs Advanced => ConfigManager.Instance.GetConfig<AdvancedConfigs>(ConfigName.AdvancedConfigs);

    public static string GetConfigPath(string fileName)
    {
        string configFolder = Path.Combine(Application.dataPath, @"..\UserData\Info+");
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);

        return Path.Combine(configFolder, fileName);
    }

    public static string GenerateComments(string chineseComment, string englishComment)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        var isChineseCulture = currentCulture.Name.StartsWith("zh");
        var comment = isChineseCulture ? chineseComment : englishComment;

        return string.Join(Environment.NewLine, comment.Split("\n", StringSplitOptions.None).Select(line => "# " + line));
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