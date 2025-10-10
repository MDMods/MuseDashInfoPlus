using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Core.Utilities;
using Object = UnityEngine.Object;

namespace MDIP.Application.Services.Scoped.Text;

public class TextObjectService : ITextObjectService
{
    private bool _disposed;

    public GameObject TextLowerLeft { get; set; }
    public GameObject TextLowerRight { get; set; }
    public GameObject TextScoreBelow { get; set; }
    public GameObject TextScoreRight { get; set; }
    public GameObject TextUpperLeft { get; set; }
    public GameObject TextUpperRight { get; set; }

    public void UpdateAllText()
    {
        TextDataService.UpdateVariables();

        if (TextLowerLeft != null && ConfigAccessor.TextFieldLowerLeft.Enabled)
            SetText(TextLowerLeft, ConfigAccessor.TextFieldLowerLeft.Text?.UnEscapeReturn());

        if (TextLowerRight != null && ConfigAccessor.TextFieldLowerRight.Enabled)
            SetText(TextLowerRight, ConfigAccessor.TextFieldLowerRight.Text?.UnEscapeReturn());

        if (TextScoreBelow != null && ConfigAccessor.TextFieldScoreBelow.Enabled)
            SetText(TextScoreBelow, ConfigAccessor.TextFieldScoreBelow.Text?.UnEscapeReturn());

        if (TextScoreRight != null && ConfigAccessor.TextFieldScoreRight.Enabled)
            SetText(TextScoreRight, ConfigAccessor.TextFieldScoreRight.Text?.UnEscapeReturn());

        if (TextUpperLeft != null && ConfigAccessor.TextFieldUpperLeft.Enabled)
            SetText(TextUpperLeft, ConfigAccessor.TextFieldUpperLeft.Text?.UnEscapeReturn());

        if (TextUpperRight != null && ConfigAccessor.TextFieldUpperRight.Enabled)
            SetText(TextUpperRight, ConfigAccessor.TextFieldUpperRight.Text?.UnEscapeReturn());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            TextLowerLeft = DestroyAndClear(TextLowerLeft);
            TextLowerRight = DestroyAndClear(TextLowerRight);
            TextScoreBelow = DestroyAndClear(TextScoreBelow);
            TextScoreRight = DestroyAndClear(TextScoreRight);
            TextUpperLeft = DestroyAndClear(TextUpperLeft);
            TextUpperRight = DestroyAndClear(TextUpperRight);
        }

        _disposed = true;
    }

    private void SetText(GameObject obj, string text)
    {
        if (obj == null)
            return;

        var textComponent = obj.GetComponent<UnityEngine.UI.Text>();
        if (textComponent == null)
            return;

        textComponent.text = TextDataService.GetFormattedText(text ?? string.Empty);
    }

    private static GameObject DestroyAndClear(GameObject obj)
    {
        if (obj != null)
            Object.Destroy(obj);
        return null;
    }

    [UsedImplicitly] [Inject] public IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public ITextDataService TextDataService { get; set; }
}