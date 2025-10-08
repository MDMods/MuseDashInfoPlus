namespace MDIP.Application.Contracts;

public interface ITextDataService
{
    void UpdateConstants();
    void UpdateVariables();
    string GetFormattedText(string originalText);
}