namespace MDIP.Utils;

public static class Helper
{
	public static bool IsRegularNote(uint noteType) => noteType is >= 1 and <= 8;
}