using MDIP.Domain.Enums;

namespace MDIP.Application.Contracts;

public interface IFontService
{
    void LoadFonts(FontType type = FontType.All);
    void UnloadFonts(FontType type = FontType.All);
    Font GetFont(FontType type);
}