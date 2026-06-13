namespace MDIP.Battle;

// A one-shot entrance for the overlay: at song start the battle panel's vertical scale eases from
// squashed to normal, so the in-battle UI "un-stretches" into place. Self-timed — a fixed eased
// duration, NOT an observation of the native animation — so there are no magic position thresholds,
// no ongoing following, and no coupling to when the song starts beyond the call to Begin. Plays only
// when the overlay is visible.
internal sealed class OverlayEntrance
{
    private const float DurationSeconds = 0.5f;
    private const float StartScaleY = 3f;

    private Transform _panel;
    private float _startTime;
    private bool _running;

    // Squash the panel and start easing it open from now.
    public void Begin(Transform panel)
    {
        _panel = panel;
        if (_panel == null)
        {
            _running = false;
            return;
        }

        _startTime = Time.time;
        _running = true;
        _panel.localScale = new(1f, StartScaleY, 1f);
    }

    public void Tick()
    {
        if (!_running || _panel == null)
            return;

        var progress = Mathf.Clamp01((Time.time - _startTime) / DurationSeconds);
        _panel.localScale = new(1f, Mathf.Lerp(StartScaleY, 1f, EaseOutCubic(progress)), 1f);

        if (progress >= 1f)
        {
            _panel.localScale = new(1f, 1f, 1f);
            _running = false;
        }
    }

    // Stops the entrance and snaps the panel back to normal — used when the overlay is hidden mid-play
    // so the panel is never left squashed, and on teardown.
    public void Cancel()
    {
        if (_running && _panel != null)
            _panel.localScale = new(1f, 1f, 1f);
        _running = false;
    }

    public void Reset()
    {
        _panel = null;
        _running = false;
    }

    private static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);
}
