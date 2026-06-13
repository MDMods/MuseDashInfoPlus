using MDIP.Core.Constants;
using MDIP.Globals;

namespace MDIP.Battle;

// The score-panel zoom follower: mirrors the native score's zoom-in/out by easing the panel's
// vertical scale, gated on the player's desired visibility. A self-contained state machine owned by
// BattleUi — given the panel + score transforms, it drives the overlay's show/hide entirely on its
// own, which is why it lives apart from the overlay-construction code.
internal sealed class ScoreZoomFollower
{
    private const float ZoomSpeed = 2f;
    private const float SignificantYChange = 10f;
    private const float AutoShowDelaySeconds = 2f;

    private Transform _panel;
    private Transform _scoreTransform;

    private float _currentScale = 3f;
    private float _targetScale = 3f;
    private float _zoomProgress;
    private float _previousY = Constants.SCORE_ZOOM_OUT_Y;
    private bool _isZooming;
    private bool _isZoomingIn;
    private bool _allowNativeZoomFollow;
    private float _autoShowEnableTime;

    public bool NativeZoomInCompleted { get; private set; }

    private float CurrentY => _scoreTransform != null ? _scoreTransform.localPosition.y : Constants.SCORE_ZOOM_OUT_Y;

    // Starts following a freshly built battle panel from the zoomed-out (hidden) state.
    public void Begin(Transform panel, Transform scoreTransform)
    {
        _panel = panel;
        _scoreTransform = scoreTransform;
        _currentScale = 3f;
        _targetScale = 3f;
        _zoomProgress = 0f;
        _previousY = Constants.SCORE_ZOOM_OUT_Y;
        _isZooming = false;
        _isZoomingIn = false;
        NativeZoomInCompleted = false;
        _allowNativeZoomFollow = false;
        _autoShowEnableTime = Time.time + AutoShowDelaySeconds;
    }

    public void Reset()
    {
        _panel = null;
        _scoreTransform = null;
        _currentScale = 3f;
        _targetScale = 3f;
        _zoomProgress = 0f;
        _previousY = Constants.SCORE_ZOOM_OUT_Y;
        _isZooming = false;
        _isZoomingIn = false;
        NativeZoomInCompleted = false;
        _allowNativeZoomFollow = false;
    }

    public void Tick()
    {
        if (_panel == null || _scoreTransform == null)
            return;

        if (!NativeZoomInCompleted && CurrentY <= Constants.SCORE_ZOOM_IN_Y + 2f)
            NativeZoomInCompleted = true;

        if (!RuntimeData.DesiredUiVisible)
        {
            ForceHide();
            _previousY = CurrentY;
            return;
        }

        if (!_allowNativeZoomFollow)
        {
            if (NativeZoomInCompleted && Time.time >= _autoShowEnableTime)
                EnableFollowAndStart();
            else
            {
                ForceHide();
                _previousY = CurrentY;
                return;
            }
        }

        if (Mathf.Abs(_currentScale - _panel.localScale.y) > 0.01f)
        {
            _currentScale = _panel.localScale.y;
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
            _panel.localScale = new(1f, _currentScale, 1f);
            if (_zoomProgress >= 1f)
            {
                _isZooming = false;
                _currentScale = _targetScale;
            }
        }
        else
        {
            _panel.localScale = new(1f, _currentScale, 1f);
        }

        _previousY = CurrentY;
    }

    // The player toggled visibility: hide at once, or (re)start the follow if the native zoom-in has
    // already happened.
    public void SetVisible(bool visible)
    {
        if (!visible)
        {
            _allowNativeZoomFollow = false;
            ForceHide();
        }
        else if (NativeZoomInCompleted)
        {
            EnableFollowAndStart();
        }
        else
        {
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

    private void ForceHide()
    {
        if (_panel != null)
            _panel.localScale = new(1f, 3f, 1f);
        _currentScale = 3f;
        _targetScale = 3f;
        _isZooming = false;
        _isZoomingIn = false;
    }

    private static float EaseInCubic(float value) => value * value * value;

    private static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);
}
