using Il2CppAssets.Scripts.UI.Panels;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Assets;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Logging;
using MDIP.Application.Services.Scheduling;
using MDIP.Application.Services.Stats;
using MDIP.Application.Services.Text;
using MDIP.Domain.Configs;
using MDIP.Domain.Enums;
using MDIP.Utils;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MDIP.Application.Services.UI;

// ReSharper disable StringLiteralTypo
public class BattleUIService : IBattleUIService
{
    private const float ZoomSpeed = 2f;
    private const float SignificantYChange = 10f;

    private Transform _currentPanel;
    private float _currentScale = 3f;
    private bool _isZooming;
    private bool _isZoomingIn;
    private float _previousY = Constants.SCORE_ZOOM_OUT_Y;
    private Transform _scoreTransform;
    private float _targetScale = 3f;

    private GameObject _textObjectTemplate;
    private float _zoomProgress;

    private float CurrentY => _scoreTransform?.localPosition.y ?? Constants.SCORE_ZOOM_OUT_Y;

    public void OnGameStart(PnlBattle instance)
    {
        if (_textObjectTemplate != null)
            return;

        GameStatsService.IsInGame = true;
        RefreshScheduler.Reset();

        try
        {
            // Make sure fonts loaded
            FontService.LoadFonts();

            var battleRoot = instance.transform.Find("PnlBattleUI").transform;
            var pnlBattleOthers = instance.transform.Find("PnlBattleUI/PnlBattleOthers")?.gameObject;
            if (pnlBattleOthers == null)
                throw new NullReferenceException("PnlBattleOthers is null");

            GameObject activeBattlePanel = null;
            for (var i = 0; i < battleRoot.childCount; i++)
            {
                var child = battleRoot.GetChild(i)?.gameObject;
                if (child?.activeSelf ?? false)
                    activeBattlePanel = child;
            }

            if (activeBattlePanel == null)
                throw new InvalidOperationException("No active battle UI panel found.");

            GameUtils.BattleUIType = activeBattlePanel.name switch
            {
                "PnlBattleOthers" => BattleUIItem.Others,
                "PnlBattleSpell" => BattleUIItem.Spell,
                "PnlBattleWisadel" => BattleUIItem.Wisadel,
                "PnlBattleBloodheir" => BattleUIItem.Bloodheir,
                _ => BattleUIItem.Unknown
            };

            activeBattlePanel.transform.localScale = new(1f, 3f, 1f);
            _currentPanel = activeBattlePanel.transform;
            _scoreTransform = activeBattlePanel.transform.Find("Score");

            _textObjectTemplate = Object.Instantiate(pnlBattleOthers.transform.Find("Score/Djmax/TxtScore_djmax").gameObject);
            Object.Destroy(_textObjectTemplate.transform.Find("ImgIconApDjmax")?.gameObject);
            Object.Destroy(_textObjectTemplate.GetComponent<ContentSizeFitter>());
            _textObjectTemplate.transform.localPosition = new(9999, 9999, -9999);

            string imgIconApPath;
            ScoreStyleType scoreStyleType;
            if (activeBattlePanel.transform.Find("Score/GC")?.gameObject.active ?? false)
            {
                scoreStyleType = ScoreStyleType.GC;
                imgIconApPath = "Score/GC/TxtScoreGC/ImgIconApGc";
            }
            else if (activeBattlePanel.transform.Find("Score/Djmax")?.gameObject.active ?? false)
            {
                scoreStyleType = ScoreStyleType.Djmax;
                imgIconApPath = "Score/Djmax/TxtScore_djmax/ImgIconApDjmax";
            }
            else if (activeBattlePanel.transform.Find("Score/ArkNight")?.gameObject.active ?? false)
            {
                scoreStyleType = ScoreStyleType.ArkNight;
                imgIconApPath = "Score/ArkNight/Area/TxtScoreArkNight/ImgIconApArkNight";
            }
            else if (activeBattlePanel.transform.Find("Score/Other/ScoreTittle/ImgEnglish")?.gameObject.active ?? false)
            {
                scoreStyleType = ScoreStyleType.OtherEN;
                imgIconApPath = "Score/Other/TxtScore/ImgIconAp";
            }
            else if (activeBattlePanel.transform.Find("Score/Other/ScoreTittle/ImgChinese")?.gameObject.active ?? false)
            {
                scoreStyleType = ScoreStyleType.OtherCN;
                imgIconApPath = "Score/Other/TxtScore/ImgIconAp";
            }
            else
            {
                scoreStyleType = ScoreStyleType.OtherEN;
                imgIconApPath = "Score/Other/TxtScore/ImgIconAp";

                Logger.Warn("Unknown score style detected. Falling back to default.");
                Logger.Warn("If you see this message, please contact the developer KARPED1EM. It's important for mod maintenance.");
                Logger.Warn("如果您看到此信息，请联系开发者 KARPED1EM！这对模组维护非常重要。");
            }

            var imgPauseRect = activeBattlePanel.transform.Find("Up/BtnPause/ImgPause").gameObject.GetComponent<RectTransform>();
            imgPauseRect.sizeDelta = new(70, 70);
            imgPauseRect.anchoredPosition = Vector2.zero;

            var imgIconAp = activeBattlePanel.transform.Find(imgIconApPath)?.gameObject;
            if (imgIconAp != null)
                imgIconAp.transform.localPosition = new(9999, 9999, -9999);

            if (ConfigAccessor.TextFieldLowerLeft.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextLowerLeft",
                    activeBattlePanel.transform.Find("Below"),
                    ConfigAccessor.TextFieldLowerLeft,
                    Constants.POS_LOWER_LEFT_TEXT,
                    false,
                    TextAnchor.LowerLeft,
                    FontStyle.Italic
                );
                TextObjectService.TextLowerLeft = obj;
            }

            if (ConfigAccessor.TextFieldLowerRight.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextLowerRight",
                    activeBattlePanel.transform.Find("Below"),
                    ConfigAccessor.TextFieldLowerRight,
                    Constants.POS_LOWER_RIGHT_TEXT,
                    false,
                    TextAnchor.LowerRight,
                    FontStyle.Italic
                );
                TextObjectService.TextLowerRight = obj;
            }

            if (ConfigAccessor.TextFieldScoreBelow.Enabled)
            {
                var offset = Constants.OFFSET_SCORE_BELOW_TEXT[scoreStyleType];
                var position = new Vector3(offset.x, offset.y == 0 ? Constants.POS_SCORE_BELOW_TEXT.y : offset.y, Constants.POS_SCORE_BELOW_TEXT.z);
                var obj = CreateTextObj(
                    "InfoPlus_TextScoreBelow",
                    activeBattlePanel.transform.Find("Score"),
                    ConfigAccessor.TextFieldScoreBelow,
                    position,
                    false,
                    TextAnchor.UpperLeft
                );
                TextObjectService.TextScoreBelow = obj;
            }

            if (ConfigAccessor.TextFieldScoreRight.Enabled && GameUtils.BattleUIType != BattleUIItem.Spell)
            {
                var parentPath = imgIconApPath[..imgIconApPath.LastIndexOf('/')];
                var parentTransform = activeBattlePanel.transform.Find(parentPath);
                var obj = CreateTextObj(
                    "InfoPlus_TextScoreRight",
                    parentTransform,
                    ConfigAccessor.TextFieldScoreRight,
                    new(Constants.POS_SCORE_RIGHT_TEXT.x, Constants.OFFSET_SCORE_RIGHT_TEXT[scoreStyleType], Constants.POS_SCORE_RIGHT_TEXT.z),
                    true,
                    TextAnchor.LowerLeft
                );
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new(1, 1);
                rect.anchorMax = new(1, 1);
                rect.pivot = new(1, 1);
                TextObjectService.TextScoreRight = obj;
            }

            if (ConfigAccessor.TextFieldUpperLeft.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextUpperLeft",
                    activeBattlePanel.transform.Find("Up"),
                    ConfigAccessor.TextFieldUpperLeft,
                    Constants.POS_UPPER_LEFT_TEXT,
                    false,
                    TextAnchor.UpperLeft
                );
                TextObjectService.TextUpperLeft = obj;
            }

            if (ConfigAccessor.TextFieldUpperRight.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextUpperRight",
                    activeBattlePanel.transform.Find("Up"),
                    ConfigAccessor.TextFieldUpperRight,
                    Constants.POS_UPPER_RIGHT_TEXT,
                    false,
                    TextAnchor.UpperRight
                );
                var text = obj.GetComponent<UnityEngine.UI.Text>();
                text.lineSpacing = 0.8f;
                obj.transform.localScale = new(1, 0.95f, 1);
                TextObjectService.TextUpperRight = obj;
            }

            GameStatsService.Init();
            TextDataService.UpdateConstants();
            GameStatsService.UpdateCurrentStats();
            TextObjectService.UpdateAllText();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    public void CheckAndZoom()
    {
        if (_currentPanel == null || _scoreTransform == null)
            return;

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

    public void Reset()
    {
        _textObjectTemplate = null;
        _currentPanel = null;
        _scoreTransform = null;
        _previousY = Constants.SCORE_ZOOM_OUT_Y;
        _isZooming = false;
        _isZoomingIn = false;
        _targetScale = 3f;
        _currentScale = 3f;
        _zoomProgress = 0f;
    }

    private static float EaseInCubic(float value) => value * value * value;
    private static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);

    private GameObject CreateTextObj(
        string objectName,
        Transform parent,
        ITextConfig config,
        Vector3 position,
        bool skipRectReset,
        TextAnchor alignment,
        FontStyle fontStyle = FontStyle.Normal)
    {
        var existing = parent.Find(objectName)?.gameObject;
        var obj = existing ?? Object.Instantiate(_textObjectTemplate, parent);
        obj.name = objectName;

        Object.Destroy(obj.transform.Find("ImgIconApDjmax")?.gameObject);
        Object.Destroy(obj.GetComponent<ContentSizeFitter>());

        if (!skipRectReset)
        {
            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new(0.5f, 0.5f);
            rectTransform.anchorMax = new(0.5f, 0.5f);
            rectTransform.pivot = new(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.sizeDelta = new(100f, 100f);
            rectTransform.localScale = Vector3.one;
        }

        var text = obj.GetComponent<UnityEngine.UI.Text>();
        text.font = config.Font switch
        {
            "Snaps Taste" => FontService.GetFont(FontType.SnapsTaste),
            "Lato" => FontService.GetFont(FontType.LatoRegular),
            "Luckiest Guy" => FontService.GetFont(FontType.LuckiestGuy),
            "Normal" => FontService.GetFont(FontType.Normal),
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

        return obj;
    }

    [UsedImplicitly] [Inject] public IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public ITextObjectService TextObjectService { get; set; }
    [UsedImplicitly] [Inject] public IFontService FontService { get; set; }
    [UsedImplicitly] [Inject] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] [Inject] public ITextDataService TextDataService { get; set; }
    [UsedImplicitly] [Inject] public IRefreshScheduler RefreshScheduler { get; set; }
    [UsedImplicitly] [Inject] public ILogger<BattleUIService> Logger { get; set; }
}