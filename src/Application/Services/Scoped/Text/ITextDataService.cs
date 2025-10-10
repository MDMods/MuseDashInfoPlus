namespace MDIP.Application.Services.Scoped.Text;

public interface ITextDataService
{
    void UpdateConstants();
    void UpdateVariables();
    string GetFormattedText(string originalText);
    void ApplyPendingConstantsRefresh();
}