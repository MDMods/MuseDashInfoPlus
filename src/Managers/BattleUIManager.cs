using Il2CppAssets.Scripts.UI.Panels;
using MDIP.Interfaces;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MDIP.Managers;

public static class BattleUIManager
{
    private const float ZoomSpeed = 2f;
    private const float SignificantYChange = 10f;
    private static GameObject _textObjectTemplate;
    private static Transform _currentPanel;
    private static Transform _scoreTransform;
    private static float _previousY = Constants.SCORE_ZOOM_OUT_Y;
    private static bool _isZooming;
    private static bool _isZoomingIn;
    private static float _targetScale = 3f;
    private static float _currentScale = 3f;
    private static float _zoomProgress;

    private static float CurrentY => _scoreTransform?.localPosition.y ?? Constants.SCORE_ZOOM_OUT_Y;

    public static void OnGameStart(PnlBattle instance)
    {
        if (_textObjectTemplate != null) return;
        MDIPMod.Reset = false;
        GameStatsManager.IsInGame = true;

        try
        {
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

            var panelName = activeBattlePanel.name;
            GameUtils.BattleUIType = panelName switch
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
                throw new InvalidOperationException("Unknown score style type.");

            var imgPauseRect = activeBattlePanel.transform.Find("Up/BtnPause/ImgPause").gameObject.GetComponent<RectTransform>();
            imgPauseRect.sizeDelta = new(70, 70);
            imgPauseRect.anchoredPosition = Vector2.zero;

            var imgIconAp = activeBattlePanel.transform.Find(imgIconApPath)?.gameObject;
            if (imgIconAp != null) imgIconAp.transform.localPosition = new(9999, 9999, -9999);

            FontUtils.LoadFonts();

            if (Configs.TextFieldLowerLeft.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextLowerLeft",
                    activeBattlePanel.transform.Find("Below"),
                    Configs.TextFieldLowerLeft,
                    Constants.POS_LOWER_LEFT_TEXT,
                    false,
                    TextAnchor.LowerLeft,
                    FontStyle.Italic
                );
                TextObjManager.TextLowerLeftObj = obj;
            }

            if (Configs.TextFieldLowerRight.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextLowerRight",
                    activeBattlePanel.transform.Find("Below"),
                    Configs.TextFieldLowerRight,
                    Constants.POS_LOWER_RIGHT_TEXT,
                    false,
                    TextAnchor.LowerRight,
                    FontStyle.Italic
                );
                TextObjManager.TextLowerRightObj = obj;
            }

            if (Configs.TextFieldScoreBelow.Enabled)
            {
                var offset = Constants.OFFSET_SCORE_BELOW_TEXT[scoreStyleType];
                var position = new Vector3(offset.x, offset.y == 0 ? Constants.POS_SCORE_BELOW_TEXT.y : offset.y, Constants.POS_SCORE_BELOW_TEXT.z);
                var obj = CreateTextObj(
                    "InfoPlus_TextScoreBelow",
                    activeBattlePanel.transform.Find("Score"),
                    Configs.TextFieldScoreBelow,
                    position,
                    false,
                    TextAnchor.UpperLeft
                );
                TextObjManager.TextScoreBelowObj = obj;
            }

            if (Configs.TextFieldScoreRight.Enabled && GameUtils.BattleUIType != BattleUIItem.Spell)
            {
                var parentPath = imgIconApPath[..imgIconApPath.LastIndexOf('/')];
                var parentTransform = activeBattlePanel.transform.Find(parentPath);
                var obj = CreateTextObj(
                    "InfoPlus_TextScoreRight",
                    parentTransform,
                    Configs.TextFieldScoreRight,
                    new(Constants.POS_SCORE_RIGHT_TEXT.x, Constants.OFFSET_SCORE_RIGHT_TEXT[scoreStyleType], Constants.POS_SCORE_RIGHT_TEXT.z),
                    true,
                    TextAnchor.LowerLeft
                );
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new(1, 1);
                rect.anchorMax = new(1, 1);
                rect.pivot = new(1, 1);
                TextObjManager.TextScoreRightObj = obj;
            }

            if (Configs.TextFieldUpperLeft.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextUpperLeft",
                    activeBattlePanel.transform.Find("Up"),
                    Configs.TextFieldUpperLeft,
                    Constants.POS_UPPER_LEFT_TEXT,
                    false,
                    TextAnchor.UpperLeft
                );
                TextObjManager.TextUpperLeftObj = obj;
            }

            if (Configs.TextFieldUpperRight.Enabled)
            {
                var obj = CreateTextObj(
                    "InfoPlus_TextUpperRight",
                    activeBattlePanel.transform.Find("Up"),
                    Configs.TextFieldUpperRight,
                    Constants.POS_UPPER_RIGHT_TEXT,
                    false,
                    TextAnchor.UpperRight
                );
                var text = obj.GetComponent<Text>();
                text.lineSpacing = 0.8f;
                obj.transform.localScale = new(1, 0.95f, 1);
                TextObjManager.TextUpperRightObj = obj;
            }

            GameStatsManager.Init();
            TextDataManager.UpdateConstants();
            GameStatsManager.UpdateCurrentStats();
            TextObjManager.UpdateAllText();
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.Error(ex.ToString());
        }
    }

    public static void CheckAndZoom()
    {
        if (_currentPanel == null || _scoreTransform == null) return;

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

    public static void Reset()
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

    private static GameObject CreateTextObj(
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

        var text = obj.GetComponent<Text>();
        text.font = config.Font switch
        {
            "Snaps Taste" => FontUtils.GetFont(FontType.SnapsTaste),
            "Lato" => FontUtils.GetFont(FontType.LatoRegular),
            "Luckiest Guy" => FontUtils.GetFont(FontType.LuckiestGuy),
            "Normal" => FontUtils.GetFont(FontType.Normal),
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
}