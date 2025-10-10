namespace MDIP.Application.Services.Scoped.Text;

public interface ITextObjectService : IDisposable
{
    GameObject TextLowerLeft { get; set; }
    GameObject TextLowerRight { get; set; }
    GameObject TextScoreBelow { get; set; }
    GameObject TextScoreRight { get; set; }
    GameObject TextUpperLeft { get; set; }
    GameObject TextUpperRight { get; set; }
    void UpdateAllText();
}