namespace MDIP.Battle;

// The overlay's entrance: a short fade-in of Info+'s OWN text, started the instant the overlay is
// built (in step with the native UI's own entrance).
//
// Why a fade and not "just ride the native animation": Info+'s text sits at the screen edges and can
// be large, while the native entrance is only a small displacement — so a text parented under the
// panel is already at its final spot, fully visible (poking out), while the native elements are still
// sliding in. Starting from alpha 0 makes the reveal impossible to pre-empt for ANY text size or
// position, and it touches only Info+'s own text (it never re-animates the native panel). The text
// still rides the panel's motion as a child, so it slides in WITH the native UI and merely fades up
// to opacity as it arrives. Self-timed (a fixed eased duration) — no observation of the native curve.
internal sealed class OverlayEntrance
{
    private const float DurationSeconds = 0.4f;

    private float _startTime;
    private bool _running;

    public bool IsRunning => _running;

    public void Begin() // caller sets alpha to 0 before the first frame renders
    {
        _startTime = Time.time;
        _running = true;
    }

    // While the fade is in progress, outputs the eased alpha (0..1) and returns true; once complete
    // (or not running) returns false so the caller stops driving it.
    public bool TryAdvance(out float alpha)
    {
        alpha = 1f;
        if (!_running)
            return false;

        var progress = Mathf.Clamp01((Time.time - _startTime) / DurationSeconds);
        alpha = EaseOutCubic(progress);
        if (progress >= 1f)
        {
            alpha = 1f;
            _running = false;
        }

        return true;
    }

    public void Stop() => _running = false;

    private static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);
}
