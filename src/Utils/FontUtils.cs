namespace MDIP.Utils;

public static class FontUtils
{
    private static IFontService Service => ModServices.GetRequiredService<IFontService>();

    public static void LoadFonts(FontType type = FontType.All) => Service.LoadFonts(type);
    public static void UnloadFonts(FontType type = FontType.All) => Service.UnloadFonts(type);
    public static Font GetFont(FontType type) => Service.GetFont(type);
}