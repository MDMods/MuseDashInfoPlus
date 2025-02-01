namespace MDIP.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigCommentZhAttribute(
    string comment
) : Attribute
{
    public string Comment { get; } = comment;
}