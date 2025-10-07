namespace MDIP.Managers;

public static class TextDataManager
{
    private static ITextDataService Service => ModServices.GetRequiredService<ITextDataService>();

    public static void UpdateConstants() => Service.UpdateConstants();
    public static void UpdateVariables() => Service.UpdateVariables();
    public static string GetFormattedText(string originalText) => Service.GetFormattedText(originalText);
}