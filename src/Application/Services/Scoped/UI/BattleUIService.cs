using Il2CppAssets.Scripts.UI.Panels;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Assets;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Application.Services.Global.Logging;
using MDIP.Application.Services.Scoped.Stats;
using MDIP.Application.Services.Scoped.Text;
using MDIP.Core.Constants;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Enums;
using MDIP.Core.Utilities;
using MDIP.Presentation;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MDIP.Application.Services.Scoped.UI;

// ReSharper disable StringLiteralTypo
public class BattleUIService : IBattleUIService
{
    public bool NativeZoomInCompleted { get; private set; }
    public bool DesiredUiVisible { get; private set; }

    private const float ZoomSpeed = 2f;
    private const float SignificantYChange = 10f;
    private const float AutoShowDelaySeconds = 2f;

    private readonly List<TextFieldBinding> _textFieldBindings = [];

    private Transform _currentPanel;
    private Transform _scoreTransform;

    private GameObject _textObjectTemplate;

    private string _imgIconApParentPath;

    private float _currentScale = 3f;
    private float _targetScale = 3f;
    private float _zoomProgress;
    private float _previousY = Constants.SCORE_ZOOM_OUT_Y;

    private bool _isZooming;
    private bool _isZoomingIn;
    private bool _isShutdown;
    private bool _isInitialized;

    private bool _allowNativeZoomFollow;
    private float _autoShowEnableTime;

    private ScoreStyleType _scoreStyleType = ScoreStyleType.Unknown;

    private float CurrentY => _scoreTransform != null ? _scoreTransform.localPosition.y : Constants.SCORE_ZOOM_OUT_Y;

    private static int _pendingApplyRequests;

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

        if (!EnsureCoreServices())
            return;

        try
        {
            FontService.LoadFonts();

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

            _scoreTransform = RequireTransform(_currentPanel, "Score", "Score");
            if (_isShutdown || _scoreTransform == null)
                return;

            _textObjectTemplate = CreateTextTemplate(pnlBattleOthers);
            if (_isShutdown || _textObjectTemplate == null)
                return;

            if (!InitializeScoreStyle(_currentPanel))
                return;

            if (!ConfigurePauseButton(_currentPanel))
                return;

            BuildTextFieldBindings();

            RefreshTextFields(false, false);
            if (_isShutdown)
                return;

            GameStatsService.Init();
            TextDataService.UpdateConstants();
            GameStatsService.UpdateCurrentStats();
            TextObjectService.UpdateAllText();

            _previousY = Constants.SCORE_ZOOM_OUT_Y;
            _currentScale = 3f;
            _targetScale = 3f;
            _isZooming = false;
            _isZoomingIn = false;
            _zoomProgress = 0f;

            NativeZoomInCompleted = false;
            _allowNativeZoomFollow = false;
            DesiredUiVisible = ConfigAccessor.Main.UiVisibleByDefault;
            _autoShowEnableTime = Time.time + AutoShowDelaySeconds;

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            FailAndShutdown("Unexpected error during battle UI initialization.", ex);
        }
    }

    public void CheckAndZoom()
    {
        if (_isShutdown || !_isInitialized || _currentPanel == null || _scoreTransform == null)
            return;

        if (!NativeZoomInCompleted)
        {
            if (CurrentY <= Constants.SCORE_ZOOM_IN_Y + 2f)
                NativeZoomInCompleted = true;
        }

        if (!DesiredUiVisible)
        {
            ForceHide();
            _previousY = CurrentY;
            return;
        }

        if (!_allowNativeZoomFollow)
        {
            if (NativeZoomInCompleted && Time.time >= _autoShowEnableTime)
            {
                EnableFollowAndStart();
            }
            else
            {
                ForceHide();
                _previousY = CurrentY;
                return;
            }
        }

        if (Mathf.Abs(_currentScale - _currentPanel.localScale.y) > 0.01f)
        {
            _currentScale = _currentPanel.localScale.y;
            _targetScale = _currentScale;
        }

        var yChange = CurrentY - _previousY;
        if (!_isZooming && Mathf.Abs(yChange) > SignificantYChange)
        {
            _isZoomingIn = yChange < 0;
            _isZooming = true;
            _zoomProgress = 0f;
            _targetScale = _isZoomingIn ? 1f : 3f;
        }

        if (_isZooming)
        {
            _zoomProgress = Mathf.Min(_zoomProgress + Time.fixedDeltaTime * ZoomSpeed, 1f);
            var easedProgress = _isZoomingIn ? EaseOutCubic(_zoomProgress) : EaseInCubic(_zoomProgress);
            _currentScale = Mathf.Lerp(_isZoomingIn ? 3f : 1f, _targetScale, easedProgress);
            _currentPanel.localScale = new(1f, _currentScale, 1f);
            if (_zoomProgress >= 1f)
            {
                _isZooming = false;
                _currentScale = _targetScale;
            }
        }
        else
        {
            _currentPanel.localScale = new(1f, _currentScale, 1f);
        }

        _previousY = CurrentY;
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

    public static void RequestApplyConfigChanges()
        => Interlocked.Increment(ref _pendingApplyRequests);

    public void SetDesiredUiVisible(bool visible)
    {
        DesiredUiVisible = visible;
        if (!visible)
        {
            _allowNativeZoomFollow = false;
            ForceHide();
        }
        else
        {
            if (NativeZoomInCompleted)
                EnableFollowAndStart();
            else
                _allowNativeZoomFollow = false;
        }
    }

    private void EnableFollowAndStart()
    {
        _allowNativeZoomFollow = true;
        _isZooming = true;
        _isZoomingIn = true;
        _zoomProgress = 0f;
        _targetScale = 1f;
    }

    private void OnConfigsUpdated()
    {
        if (_isShutdown || !_isInitialized || _currentPanel == null || _textObjectTemplate == null)
            return;

        try
        {
            RefreshTextFields(true, true);

            DesiredUiVisible = ConfigAccessor.Main.UiVisibleByDefault;
            if (!DesiredUiVisible)
                ForceHide();
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

            if (binding.EnabledResolver != null && !binding.EnabledResolver())
            {
                binding.AssignAction?.Invoke(DestroyAndClear(binding.RetrieveFunc?.Invoke()));
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
            var current = binding.RetrieveFunc?.Invoke();
            if (binding.PreserveLocalPosition && preservePositions && current != null)
                savedPosition = current.transform.localPosition;

            var position = binding.PositionResolver?.Invoke() ?? Vector3.zero;
            var obj = CreateTextObj(
                binding.ObjectName,
                parent,
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
            TextObjectService.UpdateAllText();
    }

    private void ForceHide()
    {
        if (_currentPanel != null)
            _currentPanel.localScale = new(1f, 3f, 1f);
        _currentScale = 3f;
        _targetScale = 3f;
        _isZooming = false;
        _isZoomingIn = false;
    }

    private GameObject CreateTextObj(
        string objectName,
        Transform parent,
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

        var existing = parent.Find(objectName)?.gameObject;
        var obj = existing ?? Object.Instantiate(_textObjectTemplate, parent);
        obj.name = objectName;

        var icon = obj.transform.Find("ImgIconApDjmax");
        if (icon != null)
            Object.Destroy(icon.gameObject);

        var fitter = obj.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            Object.Destroy(fitter);

        if (!skipRectReset || existing == null)
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

        text.font = config.Font switch
        {
            "Snaps Taste" => FontService.GetFont(FontType.SnapsTaste) ?? text.font,
            "Lato" => FontService.GetFont(FontType.LatoRegular) ?? text.font,
            "Luckiest Guy" => FontService.GetFont(FontType.LuckiestGuy) ?? text.font,
            "Normal" => FontService.GetFont(FontType.Normal) ?? text.font,
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

        Logger?.Warn("Unknown score style detected. Falling back to default.");
        Logger?.Warn($"If you see this message, please contact the developer {ModBuildInfo.Author}. It's important for mod maintenance.");
        Logger?.Warn($"如果您看到此信息，请联系开发者 {ModBuildInfo.Author}！这对模组维护非常重要。");

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
            () => ConfigAccessor.TextFieldLowerLeft,
            () => _currentPanel.Find("Below"),
            () => Constants.POS_LOWER_LEFT_TEXT,
            false,
            TextAnchor.LowerLeft,
            FontStyle.Italic,
            () => ConfigAccessor.TextFieldLowerLeft.Enabled,
            obj => TextObjectService.TextLowerLeft = obj,
            () => TextObjectService.TextLowerLeft,
            false,
            null));

        _textFieldBindings.Add(new(
            "InfoPlus_TextLowerRight",
            () => ConfigAccessor.TextFieldLowerRight,
            () => _currentPanel.Find("Below"),
            () => Constants.POS_LOWER_RIGHT_TEXT,
            false,
            TextAnchor.LowerRight,
            FontStyle.Italic,
            () => ConfigAccessor.TextFieldLowerRight.Enabled,
            obj => TextObjectService.TextLowerRight = obj,
            () => TextObjectService.TextLowerRight,
            false,
            null));

        _textFieldBindings.Add(new(
            "InfoPlus_TextScoreBelow",
            () => ConfigAccessor.TextFieldScoreBelow,
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
            () => ConfigAccessor.TextFieldScoreBelow.Enabled,
            obj => TextObjectService.TextScoreBelow = obj,
            () => TextObjectService.TextScoreBelow,
            false,
            null));

        _textFieldBindings.Add(new(
            "InfoPlus_TextScoreRight",
            () => ConfigAccessor.TextFieldScoreRight,
            () => string.IsNullOrEmpty(_imgIconApParentPath) ? null : _currentPanel.Find(_imgIconApParentPath),
            () => new(
                Constants.POS_SCORE_RIGHT_TEXT.x,
                Constants.OFFSET_SCORE_RIGHT_TEXT[_scoreStyleType],
                Constants.POS_SCORE_RIGHT_TEXT.z),
            true,
            TextAnchor.LowerLeft,
            FontStyle.Normal,
            () => ConfigAccessor.TextFieldScoreRight.Enabled &&
                  MusicInfoUtils.BattleUIType != BattleUIItem.Spell &&
                  !string.IsNullOrEmpty(_imgIconApParentPath),
            obj => TextObjectService.TextScoreRight = obj,
            () => TextObjectService.TextScoreRight,
            true,
            ConfigureScoreRightRect));

        _textFieldBindings.Add(new(
            "InfoPlus_TextUpperLeft",
            () => ConfigAccessor.TextFieldUpperLeft,
            () => _currentPanel.Find("Up"),
            () => Constants.POS_UPPER_LEFT_TEXT,
            false,
            TextAnchor.UpperLeft,
            FontStyle.Normal,
            () => ConfigAccessor.TextFieldUpperLeft.Enabled,
            obj => TextObjectService.TextUpperLeft = obj,
            () => TextObjectService.TextUpperLeft,
            false,
            null));

        _textFieldBindings.Add(new(
            "InfoPlus_TextUpperRight",
            () => ConfigAccessor.TextFieldUpperRight,
            () => _currentPanel.Find("Up"),
            () => Constants.POS_UPPER_RIGHT_TEXT,
            false,
            TextAnchor.UpperRight,
            FontStyle.Normal,
            () => ConfigAccessor.TextFieldUpperRight.Enabled,
            obj => TextObjectService.TextUpperRight = obj,
            () => TextObjectService.TextUpperRight,
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

        Logger?.Error(message);
        if (exception != null)
            Logger?.Error(exception);

        Logger?.Fatal($"{ModBuildInfo.Name} is unavailable due to a game update. If you contact the developer {ModBuildInfo.Author}, the compatibility update can be completed sooner.");
        Logger?.Fatal($"由于游戏更新导致 {ModBuildInfo.Name} 不可用。请联系开发者 {ModBuildInfo.Author}，以便更快完成适配。 ");

        Teardown();
        _isShutdown = true;
    }

    private void Teardown()
    {
        if (TextObjectService != null)
        {
            TextObjectService.TextLowerLeft = DestroyAndClear(TextObjectService.TextLowerLeft);
            TextObjectService.TextLowerRight = DestroyAndClear(TextObjectService.TextLowerRight);
            TextObjectService.TextScoreBelow = DestroyAndClear(TextObjectService.TextScoreBelow);
            TextObjectService.TextScoreRight = DestroyAndClear(TextObjectService.TextScoreRight);
            TextObjectService.TextUpperLeft = DestroyAndClear(TextObjectService.TextUpperLeft);
            TextObjectService.TextUpperRight = DestroyAndClear(TextObjectService.TextUpperRight);
        }

        if (_textObjectTemplate != null)
        {
            Object.Destroy(_textObjectTemplate);
            _textObjectTemplate = null;
        }

        _textFieldBindings.Clear();

        _currentPanel = null;
        _scoreTransform = null;
        _imgIconApParentPath = null;

        _currentScale = 3f;
        _targetScale = 3f;
        _zoomProgress = 0f;
        _previousY = Constants.SCORE_ZOOM_OUT_Y;
        _isZooming = false;
        _isZoomingIn = false;
        _isInitialized = false;
        NativeZoomInCompleted = false;
        _allowNativeZoomFollow = false;
        DesiredUiVisible = true;

        MusicInfoUtils.BattleUIType = BattleUIItem.Unknown;

        Interlocked.Exchange(ref _pendingApplyRequests, 0);
    }

    private bool EnsureCoreServices()
    {
        return EnsureService(ConfigAccessor, nameof(ConfigAccessor)) &&
               EnsureService(TextObjectService, nameof(TextObjectService)) &&
               EnsureService(FontService, nameof(FontService)) &&
               EnsureService(GameStatsService, nameof(GameStatsService)) &&
               EnsureService(TextDataService, nameof(TextDataService));
    }

    private bool EnsureService(object service, string name)
    {
        if (service != null)
            return true;

        FailAndShutdown($"{name} is not available.");
        return false;
    }

    private static GameObject DestroyAndClear(GameObject obj)
    {
        if (obj != null)
            Object.Destroy(obj);
        return null;
    }

    private static float EaseInCubic(float value) => value * value * value;

    private static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);

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

    [UsedImplicitly] [Inject] public IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public ITextObjectService TextObjectService { get; set; }
    [UsedImplicitly] [Inject] public IFontService FontService { get; set; }
    [UsedImplicitly] [Inject] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] [Inject] public ITextDataService TextDataService { get; set; }
    [UsedImplicitly] [Inject] public ILogger<BattleUIService> Logger { get; set; }
}