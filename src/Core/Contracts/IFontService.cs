namespace MDIP.Core.Contracts;

public interface IFontService
{
    void LoadFonts(FontType type = FontType.All);
    void UnloadFonts(FontType type = FontType.All);
    Font GetFont(FontType type);
}