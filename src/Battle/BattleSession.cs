using Il2CppAssets.Scripts.UI.Panels;

namespace MDIP.Battle;

// One battle's worth of state, owned for exactly one play of one chart. Replaces the former
// per-battle DI scope: the sub-units are plain fields wired by composition, created together when
// the battle genuinely starts and torn down together when it ends. There is never more than one
// live session, so the dispose-then-rebuild-within-a-frame race that the old scope churn produced
// cannot occur.
internal sealed class BattleSession
{
    public GameStats Stats { get; }
    public NoteRecords NoteRecords { get; }
    public NoteEvents NoteEvents { get; }
    public TextObjects TextObjects { get; }
    public VictoryScreen Victory { get; }
    public BattleUi Ui { get; }
    public RefreshScheduler Scheduler { get; }

    private bool _disposed;

    public BattleSession(PnlBattle pnl)
    {
        Stats = new GameStats();
        NoteRecords = new NoteRecords(Stats);
        NoteEvents = new NoteEvents(Stats, NoteRecords);
        TextObjects = new TextObjects(Stats);
        Victory = new VictoryScreen(Stats, NoteRecords);
        Ui = new BattleUi(Stats, TextObjects);
        Scheduler = new RefreshScheduler(Ui, Stats, TextObjects);

        // Builds the overlay and runs Stats.Init(); self-isolating (FailAndShutdown on any failure),
        // so it never throws out of the constructor.
        Ui.OnGameStart(pnl);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        // Destroys our overlay GameObjects synchronously and clears all references. The native
        // battle scene is torn down by the game on the way out, so native objects need no restore.
        Ui.Dispose();
    }
}
