namespace MDIP.Core.Contracts;

public interface ITextDataService
{
    void UpdateConstants();
    void UpdateVariables();
    string GetFormattedText(string originalText);
}