using MDIP.Core.Constants;

namespace MDIP.Battle;

// The overlay's entrance, restored verbatim from the time-tested v3.0.5 behaviour. It hides the overlay
// GEOMETRICALLY: the native battle panel is squashed to its pre-zoom pose (localScale.y = 3) BEFORE the
// six text fields are created, so the text is born off-screen — a physical guarantee with no first-frame
// flash (unlike an alpha fade, which loses a frame as the canvas captures the text before alpha 0 lands).
// The panel is held squashed until the game's native zoom-in completes (detected by watching the native
// score panel's Y), then eased open IN STEP with the native zoom. Visibility (the hotkey toggle) is the
// same scale: hidden => held squashed (the edge text is off-screen, the centred native UI is unaffected).
//
// The session lifecycle (PnlBattle.GameStart with the __runOriginal guard, the idempotent session, the
// Guard boundary) is what makes this safe under Euterpe multiplayer — this class only owns the look.
internal sealed class OverlayEntrance
{
    private const float ZoomSpeed = 2f;
    private const float SignificantYChange = 10f;
    private const float AutoShowDelaySeconds = 2f;
    private const float SquashedScaleY = 3f;

    private Transform _panel;
    private Transform _scoreTransform;

    public bool NativeZoomInCompleted { get; private set; }

    private bool _allowNativeZoomFollow;
    private float _autoShowEnableTime;

    private float _currentScale = SquashedScaleY;
    private float _targetScale = SquashedScaleY;
    private float _zoomProgress;
    private float _previousY = Constants.SCORE_ZOOM_OUT_Y;
    private bool _isZooming;
    private bool _isZoomingIn;

    private float CurrentY => _scoreTransform != null ? _scoreTransform.localPosition.y : Constants.SCORE_ZOOM_OUT_Y;

    // Squash the native panel to its pre-zoom pose. MUST be called before the overlay text is created,
    // so the text is born under an already-squashed panel and never renders at full size.
    public void Begin(Transform panel, Transform scoreTransform)
    {
        _panel = panel;
        _scoreTransform = scoreTransform;

        NativeZoomInCompleted = false;
        _allowNativeZoomFollow = false;
        _autoShowEnableTime = Time.time + AutoShowDelaySeconds;

        _currentScale = SquashedScaleY;
        _targetScale = SquashedScaleY;
        _zoomProgress = 0f;
        _isZooming = false;
        _isZoomingIn = false;
        _previousY = Constants.SCORE_ZOOM_OUT_Y;

        ForceHide();
    }

    // Per-FixedUpdate driver. `desiredVisible` is the player's hotkey/config visibility. The panel is held
    // squashed while hidden or until the native zoom-in completes, then eased open following the native.
    public void Tick(bool desiredVisible)
    {
        if (_panel == null)
            return;

        if (!NativeZoomInCompleted && CurrentY <= Constants.SCORE_ZOOM_IN_Y + 2f)
            NativeZoomInCompleted = true;

        if (!desiredVisible)
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
            _targetScale = _isZoomingIn ? 1f : SquashedScaleY;
        }

        if (_isZooming)
        {
            _zoomProgress = Mathf.Min(_zoomProgress + Time.fixedDeltaTime * ZoomSpeed, 1f);
            var easedProgress = _isZoomingIn ? EaseOutCubic(_zoomProgress) : EaseInCubic(_zoomProgress);
            _currentScale = Mathf.Lerp(_isZoomingIn ? SquashedScaleY : 1f, _targetScale, easedProgress);
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

    // The hotkey/config toggle (v3.0.5 SetDesiredUiVisible semantics): hidden snaps back to squashed;
    // shown either starts the open (if the native zoom already finished) or waits for it.
    public void OnDesiredVisibleChanged(bool visible)
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

    // Battle teardown: the game destroys the battle scene (and the panel) on exit, so there is nothing to
    // restore — just drop references.
    public void Reset()
    {
        _panel = null;
        _scoreTransform = null;
        NativeZoomInCompleted = false;
        _allowNativeZoomFollow = false;
    }

    private void ForceHide()
    {
        if (_panel != null)
            _panel.localScale = new(1f, SquashedScaleY, 1f);
        _currentScale = SquashedScaleY;
        _targetScale = SquashedScaleY;
        _isZooming = false;
        _isZoomingIn = false;
    }

    private void EnableFollowAndStart()
    {
        _allowNativeZoomFollow = true;
        _isZooming = true;
        _isZoomingIn = true;
        _zoomProgress = 0f;
        _targetScale = 1f;
    }

    private static float EaseInCubic(float value) => value * value * value;
    private static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);
}
