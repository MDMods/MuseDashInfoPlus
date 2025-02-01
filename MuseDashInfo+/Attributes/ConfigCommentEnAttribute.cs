namespace MDIP.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigCommentEnAttribute(
	string comment
) : Attribute
{
	public string Comment { get; } = comment;
}