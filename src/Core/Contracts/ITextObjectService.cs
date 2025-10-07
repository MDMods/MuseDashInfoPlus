namespace MDIP.Core.Contracts;

public interface ITextObjectService
{
    GameObject TextLowerLeft { get; set; }
    GameObject TextLowerRight { get; set; }
    GameObject TextScoreBelow { get; set; }
    GameObject TextScoreRight { get; set; }
    GameObject TextUpperLeft { get; set; }
    GameObject TextUpperRight { get; set; }
    void UpdateAllText();
    void Reset();
}