namespace MDIP.Application.Services.Text;

public interface ITextDataService
{
    void UpdateConstants();
    void UpdateVariables();
    string GetFormattedText(string originalText);
}