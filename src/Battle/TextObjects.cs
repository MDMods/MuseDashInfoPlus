using MDIP.Core.Utilities;
using MDIP.Globals;
using Object = UnityEngine.Object;

namespace MDIP.Battle;

// Holds the six overlay text GameObjects and pushes the current formatted text into them every
// refresh. Owned by a BattleSession; the GameObjects themselves are created/destroyed by BattleUi,
// which clears these references on teardown.
public class TextObjects(GameStats stats)
{
    public GameObject TextLowerLeft { get; set; }
    public GameObject TextLowerRight { get; set; }
    public GameObject TextScoreBelow { get; set; }
    public GameObject TextScoreRight { get; set; }
    public GameObject TextUpperLeft { get; set; }
    public GameObject TextUpperRight { get; set; }

    public void UpdateAllText()
    {
        TextData.UpdateVariables(stats);

        if (TextLowerLeft != null && Config.TextFieldLowerLeft.Enabled)
            SetText(TextLowerLeft, Config.TextFieldLowerLeft.Text?.UnEscapeReturn());

        if (TextLowerRight != null && Config.TextFieldLowerRight.Enabled)
            SetText(TextLowerRight, Config.TextFieldLowerRight.Text?.UnEscapeReturn());

        if (TextScoreBelow != null && Config.TextFieldScoreBelow.Enabled)
            SetText(TextScoreBelow, Config.TextFieldScoreBelow.Text?.UnEscapeReturn());

        if (TextScoreRight != null && Config.TextFieldScoreRight.Enabled)
            SetText(TextScoreRight, Config.TextFieldScoreRight.Text?.UnEscapeReturn());

        if (TextUpperLeft != null && Config.TextFieldUpperLeft.Enabled)
            SetText(TextUpperLeft, Config.TextFieldUpperLeft.Text?.UnEscapeReturn());

        if (TextUpperRight != null && Config.TextFieldUpperRight.Enabled)
            SetText(TextUpperRight, Config.TextFieldUpperRight.Text?.UnEscapeReturn());
    }

    // Shows or hides the overlay by toggling the live text objects (disabled fields stay null/hidden).
    public void SetVisible(bool visible)
    {
        SetActive(TextLowerLeft, visible);
        SetActive(TextLowerRight, visible);
        SetActive(TextScoreBelow, visible);
        SetActive(TextScoreRight, visible);
        SetActive(TextUpperLeft, visible);
        SetActive(TextUpperRight, visible);
    }

    private static void SetActive(GameObject obj, bool visible)
    {
        if (obj != null)
            obj.SetActive(visible);
    }

    // Sets the alpha of every live text object (and its outline) — drives the entrance fade-in.
    public void SetAlpha(float alpha)
    {
        SetAlpha(TextLowerLeft, alpha);
        SetAlpha(TextLowerRight, alpha);
        SetAlpha(TextScoreBelow, alpha);
        SetAlpha(TextScoreRight, alpha);
        SetAlpha(TextUpperLeft, alpha);
        SetAlpha(TextUpperRight, alpha);
    }

    private static void SetAlpha(GameObject obj, float alpha)
    {
        if (obj == null)
            return;

        var text = obj.GetComponent<UnityEngine.UI.Text>();
        if (text != null)
        {
            var color = text.color;
            color.a = alpha;
            text.color = color;
        }

        var outline = obj.GetComponent<UnityEngine.UI.Outline>();
        if (outline != null)
        {
            var effect = outline.effectColor;
            effect.a = alpha;
            outline.effectColor = effect;
        }
    }

    // Destroys any live text GameObjects and clears the references. Idempotent.
    public void DestroyAll()
    {
        TextLowerLeft = DestroyAndClear(TextLowerLeft);
        TextLowerRight = DestroyAndClear(TextLowerRight);
        TextScoreBelow = DestroyAndClear(TextScoreBelow);
        TextScoreRight = DestroyAndClear(TextScoreRight);
        TextUpperLeft = DestroyAndClear(TextUpperLeft);
        TextUpperRight = DestroyAndClear(TextUpperRight);
    }

    private static void SetText(GameObject obj, string text)
    {
        if (obj == null)
            return;

        var textComponent = obj.GetComponent<UnityEngine.UI.Text>();
        if (textComponent == null)
            return;

        textComponent.text = TextData.GetFormattedText(text ?? string.Empty);
    }

    private static GameObject DestroyAndClear(GameObject obj)
    {
        if (obj != null)
            Object.Destroy(obj);
        return null;
    }
}
