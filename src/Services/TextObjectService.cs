using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MDIP.Services;

public class TextObjectService(ITextDataService textDataService) : ITextObjectService
{
    private readonly ITextDataService _textDataService = textDataService ?? throw new ArgumentNullException(nameof(textDataService));
    private long _lastUpdateTick;
    private GameObject _textLowerLeft;
    private GameObject _textLowerRight;
    private GameObject _textScoreBelow;
    private GameObject _textScoreRight;
    private GameObject _textUpperLeft;
    private GameObject _textUpperRight;

    public GameObject TextLowerLeft
    {
        get => _textLowerLeft;
        set => _textLowerLeft = value;
    }

    public GameObject TextLowerRight
    {
        get => _textLowerRight;
        set => _textLowerRight = value;
    }

    public GameObject TextScoreBelow
    {
        get => _textScoreBelow;
        set => _textScoreBelow = value;
    }

    public GameObject TextScoreRight
    {
        get => _textScoreRight;
        set => _textScoreRight = value;
    }

    public GameObject TextUpperLeft
    {
        get => _textUpperLeft;
        set => _textUpperLeft = value;
    }

    public GameObject TextUpperRight
    {
        get => _textUpperRight;
        set => _textUpperRight = value;
    }

    public void UpdateAllText()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (now - _lastUpdateTick < Configs.Advanced.DataRefreshIntervalLimit)
            return;

        _lastUpdateTick = now;
        _textDataService.UpdateVariables();

        if (_textLowerLeft != null && Configs.TextFieldLowerLeft.Enabled)
        {
            var content = Configs.TextFieldLowerLeft.Text?.UnEscapeReturn() ?? string.Empty;
            SetText(_textLowerLeft, _textDataService.GetFormattedText(content));
        }

        if (_textLowerRight != null && Configs.TextFieldLowerRight.Enabled)
        {
            var content = Configs.TextFieldLowerRight.Text?.UnEscapeReturn() ?? string.Empty;
            SetText(_textLowerRight, _textDataService.GetFormattedText(content));
        }

        if (_textScoreBelow != null && Configs.TextFieldScoreBelow.Enabled)
        {
            var content = Configs.TextFieldScoreBelow.Text?.UnEscapeReturn() ?? string.Empty;
            SetText(_textScoreBelow, _textDataService.GetFormattedText(content));
        }

        if (_textScoreRight != null && Configs.TextFieldScoreRight.Enabled)
        {
            var content = Configs.TextFieldScoreRight.Text?.UnEscapeReturn() ?? string.Empty;
            SetText(_textScoreRight, _textDataService.GetFormattedText(content));
        }

        if (_textUpperLeft != null && Configs.TextFieldUpperLeft.Enabled)
        {
            var content = Configs.TextFieldUpperLeft.Text?.UnEscapeReturn() ?? string.Empty;
            SetText(_textUpperLeft, _textDataService.GetFormattedText(content));
        }

        if (_textUpperRight != null && Configs.TextFieldUpperRight.Enabled)
        {
            var content = Configs.TextFieldUpperRight.Text?.UnEscapeReturn() ?? string.Empty;
            SetText(_textUpperRight, _textDataService.GetFormattedText(content));
        }
    }

    public void Reset()
    {
        DestroyAndClear(ref _textLowerLeft);
        DestroyAndClear(ref _textLowerRight);
        DestroyAndClear(ref _textScoreBelow);
        DestroyAndClear(ref _textScoreRight);
        DestroyAndClear(ref _textUpperLeft);
        DestroyAndClear(ref _textUpperRight);
        _lastUpdateTick = 0;
    }

    private static void SetText(GameObject obj, string text)
    {
        if (obj == null)
            return;

        var textComponent = obj.GetComponent<Text>();
        if (textComponent == null)
            return;

        textComponent.text = text;
    }

    private static void DestroyAndClear(ref GameObject obj)
    {
        if (obj == null)
            return;

        Object.Destroy(obj);
        obj = null;
    }
}