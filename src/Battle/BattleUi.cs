using Il2CppAssets.Scripts.UI.Panels;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.GameCore.Managers;
using MDIP.Core.Constants;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Enums;
using MDIP.Core.Utilities;
using MDIP.Globals;
using MDIP.Presentation;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MDIP.Battle;

// Builds the in-battle overlay (clones a score-text template, spawns the six configured text fields
// under the native panel) and keeps the score-skill indicator offset in sync. The score-panel zoom
// follow is delegated to ScoreZoomFollower; this class is the construction + per-frame coordination.
//
// Robustness: text objects are reused from the tracked TextObjects references (never re-found by
// name), so a stale or pending-destroy GameObject can never be picked up — the multiplayer
// double-start bug class is closed at the source. Build failures shut the overlay down cleanly
// without throwing into the game.
public class BattleUi(GameStats stats, TextObjects textObjects)
{
    private bool _disposed;

    private readonly ScoreZoomFollower _zoom = new();

    private readonly List<TextFieldBinding> _textFieldBindings = [];
    private int _pendingApplyRequests;

    private Transform _currentPanel;

    private GameObject _textObjectTemplate;

    private string _imgIconApParentPath;

    private bool _isShutdown;
    private bool _isInitialized;

    private bool _isMissToGreat;
    private bool _isGreatToPerfect;
    private float _skillOffset;
    private bool _skillOffsetInitialized;

    private ScoreStyleType _scoreStyleType = ScoreStyleType.Unknown;

    private bool? _lastUiVisibleByDefault;

    public bool NativeZoomInCompleted => _zoom.NativeZoomInCompleted;

    public void OnGameStart(PnlBattle instance)
    {
        if (_isShutdown)
            return;

        Teardown();

        if (instance == null)
        {
            FailAndShutdown("PnlBattle instance is null.");
            return;
        }

        try
        {
            Fonts.LoadFonts();

            var battleRoot = RequireTransform(instance.transform, "PnlBattleUI", "PnlBattleUI root");
            if (_isShutdown || battleRoot == null)
                return;

            var pnlBattleOthers = RequireTransform(instance.transform, "PnlBattleUI/PnlBattleOthers", "PnlBattleOthers");
            if (_isShutdown || pnlBattleOthers == null)
                return;

            var activeBattlePanel = FindActivePanel(battleRoot);
            if (activeBattlePanel == null)
            {
                FailAndShutdown("No active battle UI panel found.");
                return;
            }

            MusicInfoUtils.BattleUIType = activeBattlePanel.name switch
            {
                "PnlBattleOthers" => BattleUIItem.Others,
                "PnlBattleSpell" => BattleUIItem.Spell,
                "PnlBattleWisadel" => BattleUIItem.Wisadel,
                "PnlBattleBloodheir" => BattleUIItem.Bloodheir,
                _ => BattleUIItem.Unknown
            };

            _currentPanel = activeBattlePanel.transform;
            if (_currentPanel == null)
            {
                FailAndShutdown("Active battle panel transform is missing.");
                return;
            }

            _currentPanel.localScale = new(1f, 3f, 1f);

            var scoreTransform = RequireTransform(_currentPanel, "Score", "Score");
            if (_isShutdown || scoreTransform == null)
                return;

            _textObjectTemplate = CreateTextTemplate(pnlBattleOthers);
            if (_isShutdown || _textObjectTemplate == null)
                return;

            if (!InitializeScoreStyle(_currentPanel))
                return;

            if (!ConfigurePauseButton(_currentPanel))
                return;

            _lastUiVisibleByDefault = Config.Main.UiVisibleByDefault;

            BuildTextFieldBindings();

            RefreshTextFields(false, false);
            if (_isShutdown)
                return;

            stats.Init();
            TextData.UpdateConstants(stats);
            stats.UpdateCurrentStats();
            textObjects.UpdateAllText();

            _zoom.Begin(_currentPanel, scoreTransform);

            _isMissToGreat = false;
            _isGreatToPerfect = false;
            _skillOffset = 0f;
            _skillOffsetInitialized = false;

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            FailAndShutdown("Unexpected error during battle UI initialization.", ex);
        }
    }

    public void CheckAndZoom()
    {
        if (_isShutdown || !_isInitialized)
            return;

        UpdateSkillOffset();
        _zoom.Tick();
    }

    // Horizontal shift applied per visible auto-avoid skill icon. Kept here (not split out) because it
    // shares the text-field offset state with the upper-left binding.
    private const float SkillIconWidth = 80f;

    private void UpdateSkillOffset()
    {
        if (_isShutdown || !_isInitialized || textObjects.TextUpperLeft == null)
            return;

        // Feature disabled: clear any offset previously applied so the text snaps back,
        // and reset state so re-enabling mid-battle re-initializes cleanly.
        if (!Config.Main.PushIndicatorOnSkill)
        {
            if (_skillOffsetInitialized || _skillOffset != 0f)
            {
                _skillOffset = 0f;
                _isMissToGreat = false;
                _isGreatToPerfect = false;
                _skillOffsetInitialized = false;
                ApplyUpperLeftPosition();
            }
            return;
        }

        var battleProperty = Singleton<BattleProperty>.instance;
        if (battleProperty == null)
            return;

        if (!_skillOffsetInitialized)
        {
            _skillOffset = 0f;
            _isMissToGreat = false;
            _isGreatToPerfect = false;

            if (battleProperty.missToGreat != 0)
            {
                _isMissToGreat = true;
                _skillOffset += SkillIconWidth;
            }
            if (battleProperty.greatToPerfect != 0)
            {
                _isGreatToPerfect = true;
                _skillOffset += SkillIconWidth;
            }

            _skillOffsetInitialized = true;
            ApplyUpperLeftPosition();
            return;
        }

        var changed = false;

        if (_isMissToGreat && battleProperty.missToGreat < 1)
        {
            _skillOffset -= SkillIconWidth;
            _isMissToGreat = false;
            changed = true;
        }

        if (_isGreatToPerfect && battleProperty.greatToPerfect < 1)
        {
            _skillOffset -= SkillIconWidth;
            _isGreatToPerfect = false;
            changed = true;
        }

        if (changed)
            ApplyUpperLeftPosition();
    }

    private void ApplyUpperLeftPosition()
    {
        var textObj = textObjects.TextUpperLeft;
        var config = Config.TextFieldUpperLeft;
        if (textObj == null || config == null)
            return;

        textObj.transform.localPosition = new Vector3(
            Constants.POS_UPPER_LEFT_TEXT.x + _skillOffset + config.OffsetX,
            Constants.POS_UPPER_LEFT_TEXT.y + config.OffsetY,
            Constants.POS_UPPER_LEFT_TEXT.z);
    }

    public void QueueApplyConfigChanges()
    {
        if (_isShutdown)
            return;

        Interlocked.Increment(ref _pendingApplyRequests);
    }

    public void ApplyPendingConfigChanges()
    {
        if (_isShutdown || !_isInitialized)
        {
            Interlocked.Exchange(ref _pendingApplyRequests, 0);
            return;
        }

        if (Interlocked.Exchange(ref _pendingApplyRequests, 0) <= 0)
            return;

        OnConfigsUpdated();
    }

    public void SetDesiredUiVisible(bool visible)
    {
        RuntimeData.SetDesiredUiVisible(visible);
        _zoom.SetVisible(visible);
    }

    // Tears the overlay down deterministically: destroys the owned text objects and template and
    // clears all state. Idempotent. Called at battle end / scene change by the owning session.
    public void Dispose()
    {
        if (_disposed)
            return;

        Teardown();
        _disposed = true;
    }

    private void OnConfigsUpdated()
    {
        if (_isShutdown || !_isInitialized || _currentPanel == null || _textObjectTemplate == null)
            return;

        try
        {
            RefreshTextFields(true, true);

            var currentDefault = Config.Main.UiVisibleByDefault;
            if (_lastUiVisibleByDefault == null)
            {
                _lastUiVisibleByDefault = currentDefault;
            }
            else if (_lastUiVisibleByDefault.Value != currentDefault)
            {
                _lastUiVisibleByDefault = currentDefault;
                SetDesiredUiVisible(currentDefault);
            }
        }
        catch (Exception ex)
        {
            FailAndShutdown("Unexpected error while applying configuration changes.", ex);
        }
    }

    private void RefreshTextFields(bool preservePositions, bool triggerTextRefresh)
    {
        if (_isShutdown)
            return;

        foreach (var binding in _textFieldBindings)
        {
            if (_isShutdown)
                return;

            var current = binding.RetrieveFunc?.Invoke();

            if (binding.EnabledResolver != null && !binding.EnabledResolver())
            {
                binding.AssignAction?.Invoke(DestroyAndClear(current));
                continue;
            }

            var config = binding.ConfigResolver?.Invoke();
            if (config == null)
            {
                FailAndShutdown($"Configuration missing for {binding.ObjectName}.");
                return;
            }

            var parent = binding.ParentResolver?.Invoke();
            if (parent == null)
            {
                FailAndShutdown($"Parent transform missing for {binding.ObjectName}.");
                return;
            }

            Vector3? savedPosition = null;
            if (binding.PreserveLocalPosition && preservePositions && current != null)
                savedPosition = current.transform.localPosition;

            var position = binding.PositionResolver?.Invoke() ?? Vector3.zero;
            var obj = CreateTextObj(
                binding.ObjectName,
                parent,
                current,
                config,
                position,
                binding.SkipRectReset,
                binding.Alignment,
                binding.FontStyle);

            if (_isShutdown || obj == null)
                return;

            binding.PostCreateAction?.Invoke(obj);
            if (_isShutdown)
                return;

            if (savedPosition.HasValue)
                obj.transform.localPosition = savedPosition.Value;

            binding.AssignAction?.Invoke(obj);
        }

        if (triggerTextRefresh && !_isShutdown)
            textObjects.UpdateAllText();
    }

    // Reuses the caller-tracked `existing` GameObject when present (the config-reapply path);
    // otherwise instantiates a fresh one from the template. It never re-discovers objects by name,
    // so a deferred-destroy GameObject from a prior lifecycle can't be picked up.
    private GameObject CreateTextObj(
        string objectName,
        Transform parent,
        GameObject existing,
        ITextConfig config,
        Vector3 position,
        bool skipRectReset,
        TextAnchor alignment,
        FontStyle fontStyle)
    {
        if (_textObjectTemplate == null)
        {
            FailAndShutdown("Text object template is missing.");
            return null;
        }

        if (config == null)
        {
            FailAndShutdown($"Configuration missing for {objectName}.");
            return null;
        }

        var obj = existing != null ? existing : Object.Instantiate(_textObjectTemplate, parent);
        obj.name = objectName;

        var icon = obj.transform.Find("ImgIconApDjmax");
        if (icon != null)
            Object.Destroy(icon.gameObject);

        var fitter = obj.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            Object.Destroy(fitter);

        if (!skipRectReset)
        {
            var rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                FailAndShutdown($"RectTransform is missing for {objectName}.");
                return null;
            }

            rectTransform.anchorMin = new(0.5f, 0.5f);
            rectTransform.anchorMax = new(0.5f, 0.5f);
            rectTransform.pivot = new(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.sizeDelta = new(100f, 100f);
            rectTransform.localScale = Vector3.one;
        }

        var text = obj.GetComponent<UnityEngine.UI.Text>();
        if (text == null)
        {
            FailAndShutdown($"Text component is missing for {objectName}.");
            return null;
        }

        text.font = config.Font.Trim().ToLower().Replace(" ", "") switch
        {
            "snapstaste" => Fonts.GetFont(FontType.SnapsTaste) ?? text.font,
            "lato" => Fonts.GetFont(FontType.LatoRegular) ?? text.font,
            "luckiestguy" => Fonts.GetFont(FontType.LuckiestGuy) ?? text.font,
            "normal" => Fonts.GetFont(FontType.Normal) ?? text.font,
            _ => text.font
        };

        text.alignment = alignment;
        text.fontStyle = fontStyle;
        text.fontSize = config.FontSize;
        text.color = config.FontColor.ToColor();

        var existingOutline = obj.GetComponent<Outline>();
        if (config.FontOutlineEnabled)
        {
            var outline = existingOutline ?? obj.AddComponent<Outline>();
            outline.effectColor = config.FontOutlineColor.ToColor();
            outline.effectDistance = new(config.FontOutlineWidth, config.FontOutlineWidth);
        }
        else if (existingOutline != null)
        {
            Object.Destroy(existingOutline);
        }

        obj.transform.localPosition = new(
            position.x + config.OffsetX,
            position.y + config.OffsetY,
            position.z);

        obj.SetActive(true);

        return obj;
    }

    private GameObject CreateTextTemplate(Transform pnlBattleOthers)
    {
        var templateTransform = RequireTransform(pnlBattleOthers, "Score/Djmax/TxtScore_djmax", "score text template");
        if (_isShutdown || templateTransform == null)
            return null;

        var template = Object.Instantiate(templateTransform.gameObject);
        template.name = "InfoPlus_TextTemplate";

        var icon = template.transform.Find("ImgIconApDjmax");
        if (icon != null)
            Object.Destroy(icon.gameObject);

        var fitter = template.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            Object.Destroy(fitter);

        template.transform.localPosition = new(9999f, 9999f, -9999f);

        return template;
    }

    private bool InitializeScoreStyle(Transform panel)
    {
        if (TryConfigureScoreStyle(panel, "Score/GC", ScoreStyleType.GC, "Score/GC/TxtScoreGC/ImgIconApGc"))
            return true;
        if (TryConfigureScoreStyle(panel, "Score/Djmax", ScoreStyleType.Djmax, "Score/Djmax/TxtScore_djmax/ImgIconApDjmax"))
            return true;
        if (TryConfigureScoreStyle(panel, "Score/ArkNight", ScoreStyleType.ArkNight, "Score/ArkNight/Area/TxtScoreArkNight/ImgIconApArkNight"))
            return true;
        if (TryConfigureScoreStyle(panel, "Score/Other/ScoreTittle/ImgEnglish", ScoreStyleType.OtherEN, "Score/Other/TxtScore/ImgIconAp"))
            return true;
        if (TryConfigureScoreStyle(panel, "Score/Other/ScoreTittle/ImgChinese", ScoreStyleType.OtherCN, "Score/Other/TxtScore/ImgIconAp"))
            return true;

        _scoreStyleType = ScoreStyleType.OtherEN;
        if (!TrySetupScoreIcon("Score/Other/TxtScore/ImgIconAp"))
            return false;

        Log.Warn("Unknown score style detected. Falling back to default.");
        Log.Warn($"If you see this message, please contact the developer {ModBuildInfo.Author}. It's important for mod maintenance.");
        Log.Warn($"如果您看到此信息，请联系开发者 {ModBuildInfo.Author}！这对模组维护非常重要。");

        return true;
    }

    private bool TryConfigureScoreStyle(Transform panel, string detectionPath, ScoreStyleType styleType, string iconPath)
    {
        if (!IsActive(panel, detectionPath))
            return false;

        _scoreStyleType = styleType;
        return TrySetupScoreIcon(iconPath);
    }

    private bool TrySetupScoreIcon(string iconPath)
    {
        if (_currentPanel == null)
        {
            FailAndShutdown("Current panel is unavailable while configuring score icon.");
            return false;
        }

        var iconTransform = RequireTransform(_currentPanel, iconPath, $"score icon at {iconPath}");
        if (_isShutdown || iconTransform == null)
            return false;

        var parentPathIndex = iconPath.LastIndexOf('/');
        if (parentPathIndex <= 0)
        {
            FailAndShutdown("Score icon path is invalid.");
            return false;
        }

        var parentPath = iconPath[..parentPathIndex];
        var parentTransform = RequireTransform(_currentPanel, parentPath, $"score icon parent at {parentPath}");
        if (_isShutdown || parentTransform == null)
            return false;

        iconTransform.localPosition = new(9999f, 9999f, -9999f);
        _imgIconApParentPath = parentPath;

        return true;
    }

    private bool ConfigurePauseButton(Transform panel)
    {
        var imgPauseTransform = RequireTransform(panel, "Up/BtnPause/ImgPause", "pause button image");
        if (_isShutdown || imgPauseTransform == null)
            return false;

        var rect = imgPauseTransform.GetComponent<RectTransform>();
        if (rect == null)
        {
            FailAndShutdown("Pause button RectTransform is missing.");
            return false;
        }

        rect.sizeDelta = new(70f, 70f);
        rect.anchoredPosition = Vector2.zero;
        return true;
    }

    private void BuildTextFieldBindings()
    {
        _textFieldBindings.Clear();

        _textFieldBindings.Add(new(
            "InfoPlus_TextLowerLeft",
            () => Config.TextFieldLowerLeft,
            () => _currentPanel.Find("Below"),
            () => Constants.POS_LOWER_LEFT_TEXT,
            false,
            TextAnchor.LowerLeft,
            FontStyle.Italic,
            () => Config.TextFieldLowerLeft.Enabled,
            obj => textObjects.TextLowerLeft = obj,
            () => textObjects.TextLowerLeft,
            false,
            null));

        _textFieldBindings.Add(new(
            "InfoPlus_TextLowerRight",
            () => Config.TextFieldLowerRight,
            () => _currentPanel.Find("Below"),
            () => Constants.POS_LOWER_RIGHT_TEXT,
            false,
            TextAnchor.LowerRight,
            FontStyle.Italic,
            () => Config.TextFieldLowerRight.Enabled,
            obj => textObjects.TextLowerRight = obj,
            () => textObjects.TextLowerRight,
            false,
            null));

        _textFieldBindings.Add(new(
            "InfoPlus_TextScoreBelow",
            () => Config.TextFieldScoreBelow,
            () => _currentPanel.Find("Score"),
            () =>
            {
                var offset = Constants.OFFSET_SCORE_BELOW_TEXT[_scoreStyleType];
                var y = Math.Abs(offset.y) < float.Epsilon ? Constants.POS_SCORE_BELOW_TEXT.y : offset.y;
                return new(offset.x, y, Constants.POS_SCORE_BELOW_TEXT.z);
            },
            false,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            () => Config.TextFieldScoreBelow.Enabled,
            obj => textObjects.TextScoreBelow = obj,
            () => textObjects.TextScoreBelow,
            false,
            null));

        _textFieldBindings.Add(new(
            "InfoPlus_TextScoreRight",
            () => Config.TextFieldScoreRight,
            () => string.IsNullOrEmpty(_imgIconApParentPath) ? null : _currentPanel.Find(_imgIconApParentPath),
            () => new(
                Constants.POS_SCORE_RIGHT_TEXT.x,
                Constants.OFFSET_SCORE_RIGHT_TEXT[_scoreStyleType],
                Constants.POS_SCORE_RIGHT_TEXT.z),
            true,
            TextAnchor.LowerLeft,
            FontStyle.Normal,
            () => Config.TextFieldScoreRight.Enabled &&
                  MusicInfoUtils.BattleUIType != BattleUIItem.Spell &&
                  !string.IsNullOrEmpty(_imgIconApParentPath),
            obj => textObjects.TextScoreRight = obj,
            () => textObjects.TextScoreRight,
            true,
            ConfigureScoreRightRect));

        _textFieldBindings.Add(new(
            "InfoPlus_TextUpperLeft",
            () => Config.TextFieldUpperLeft,
            () => _currentPanel.Find("Up"),
            () => new Vector3(
                Constants.POS_UPPER_LEFT_TEXT.x + _skillOffset,
                Constants.POS_UPPER_LEFT_TEXT.y,
                Constants.POS_UPPER_LEFT_TEXT.z),
            false,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            () => Config.TextFieldUpperLeft.Enabled,
            obj => textObjects.TextUpperLeft = obj,
            () => textObjects.TextUpperLeft,
            false,
            null));

        _textFieldBindings.Add(new(
            "InfoPlus_TextUpperRight",
            () => Config.TextFieldUpperRight,
            () => _currentPanel.Find("Up"),
            () => Constants.POS_UPPER_RIGHT_TEXT,
            false,
            TextAnchor.UpperRight,
            FontStyle.Normal,
            () => Config.TextFieldUpperRight.Enabled,
            obj => textObjects.TextUpperRight = obj,
            () => textObjects.TextUpperRight,
            false,
            ConfigureUpperRightText));
    }

    private static GameObject FindActivePanel(Transform battleRoot)
    {
        GameObject active = null;
        for (var i = 0; i < battleRoot.childCount; i++)
        {
            var child = battleRoot.GetChild(i);
            if (child != null && child.gameObject.activeSelf)
                active = child.gameObject;
        }

        return active;
    }

    private static bool IsActive(Transform root, string path)
    {
        var target = root.Find(path);
        return target != null && target.gameObject.activeSelf;
    }

    private Transform RequireTransform(Transform root, string path, string description)
    {
        if (root == null)
        {
            FailAndShutdown($"Missing root transform while locating {description}.");
            return null;
        }

        var result = root.Find(path);
        if (result == null)
            FailAndShutdown($"Missing transform at path '{path}' for {description}.");

        return result;
    }

    private void ConfigureScoreRightRect(GameObject obj)
    {
        if (_isShutdown || obj == null)
            return;

        var rect = obj.GetComponent<RectTransform>();
        if (rect == null)
        {
            FailAndShutdown("Score right text RectTransform is missing.");
            return;
        }

        rect.anchorMin = new(1f, 1f);
        rect.anchorMax = new(1f, 1f);
        rect.pivot = new(1f, 1f);
    }

    private void ConfigureUpperRightText(GameObject obj)
    {
        if (_isShutdown || obj == null)
            return;

        var text = obj.GetComponent<UnityEngine.UI.Text>();
        if (text == null)
        {
            FailAndShutdown("Upper right text component is missing.");
            return;
        }

        text.lineSpacing = 0.8f;
        obj.transform.localScale = new(1f, 0.95f, 1f);
    }

    private void FailAndShutdown(string message, Exception exception = null)
    {
        if (_isShutdown)
            return;

        Log.Error(message);
        if (exception != null)
            Log.Error(exception);

        Log.Fatal($"{ModBuildInfo.Name} is unavailable due to a game update. If you contact the developer {ModBuildInfo.Author}, the compatibility update can be completed sooner.");
        Log.Fatal($"由于游戏更新导致 {ModBuildInfo.Name} 不可用。请联系开发者 {ModBuildInfo.Author}，以便更快完成适配。 ");

        Teardown();
        _isShutdown = true;
    }

    private void Teardown()
    {
        textObjects.DestroyAll();

        if (_textObjectTemplate != null)
        {
            Object.Destroy(_textObjectTemplate);
            _textObjectTemplate = null;
        }

        _textFieldBindings.Clear();

        _currentPanel = null;
        _imgIconApParentPath = null;

        _zoom.Reset();

        _isInitialized = false;

        MusicInfoUtils.BattleUIType = BattleUIItem.Unknown;

        _lastUiVisibleByDefault = null;

        _isMissToGreat = false;
        _isGreatToPerfect = false;
        _skillOffset = 0f;
        _skillOffsetInitialized = false;

        Interlocked.Exchange(ref _pendingApplyRequests, 0);
    }

    private static GameObject DestroyAndClear(GameObject obj)
    {
        if (obj != null)
            Object.Destroy(obj);
        return null;
    }

    private sealed class TextFieldBinding(
        string objectName,
        Func<ITextConfig> configResolver,
        Func<Transform> parentResolver,
        Func<Vector3> positionResolver,
        bool skipRectReset,
        TextAnchor alignment,
        FontStyle fontStyle,
        Func<bool> enabledResolver,
        Action<GameObject> assignAction,
        Func<GameObject> retrieveFunc,
        bool preserveLocalPosition,
        Action<GameObject> postCreateAction
    )
    {
        public string ObjectName { get; } = objectName;
        public Func<ITextConfig> ConfigResolver { get; } = configResolver;
        public Func<Transform> ParentResolver { get; } = parentResolver;
        public Func<Vector3> PositionResolver { get; } = positionResolver;
        public bool SkipRectReset { get; } = skipRectReset;
        public TextAnchor Alignment { get; } = alignment;
        public FontStyle FontStyle { get; } = fontStyle;
        public Func<bool> EnabledResolver { get; } = enabledResolver;
        public Action<GameObject> AssignAction { get; } = assignAction;
        public Func<GameObject> RetrieveFunc { get; } = retrieveFunc;
        public bool PreserveLocalPosition { get; } = preserveLocalPosition;
        public Action<GameObject> PostCreateAction { get; } = postCreateAction;
    }
}
