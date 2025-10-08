using System.Globalization;
using System.Text;

namespace MDIP.Infrastructure.Configuration;

public static class ConfigCommentFormatter
{
    public static string GenerateComments(string chineseComment, string englishComment)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        var isChineseCulture = currentCulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
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