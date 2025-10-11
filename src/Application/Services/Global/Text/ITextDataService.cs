namespace MDIP.Application.Services.Global.Text;

public interface ITextDataService
{
    void UpdateConstants();
    void UpdateVariables();
    string GetFormattedText(string originalText);
    void ApplyPendingConstantsRefresh();
}