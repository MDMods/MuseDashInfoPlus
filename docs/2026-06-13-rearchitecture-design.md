# Info+ Re-architecture Design

Date: 2026-06-13
Branch: `refactor/rearchitecture` (from `main` @ v3.0.5)
Status: approved direction (Approach 1), implementation in progress

## 1. Goals

1. **Fix the multiplayer UI-disappearance bug** at its root: with Euterpe multiplayer active,
   Info+ probabilistically caused all native battle UI to vanish and its own UI to never appear.
2. **Make Info+ robust to other mods** manipulating the game's lifecycle — not Euterpe-specific
   patches, but a lifecycle that tolerates any mod suppressing/re-invoking the methods Info+ hooks.
3. **One clean ecosystem abstraction** (`ChartSource`) covering vanilla, CustomAlbums (CAM), and
   Euterpe — replacing the scattered, implicit chart-source assumptions.
4. **Repay the v3.0.0 tech debt**: the DI container is the source of the lifecycle fragility and is
   over-engineered for a UI-overlay mod. Remove it in favour of explicit, readable composition.

Feature set, in-game UI, config surface, and placeholder system are **preserved**. This is an
internal re-architecture, not a feature change.

## 2. Why the current architecture fails (evidence)

### 2.1 The MP bug — duplicate GameScope + deferred-destroy reuse

- `PnlBattleGameStartPatch` is a Harmony **postfix with no `__runOriginal` guard**. It calls
  `ModServiceConfigurator.CreateGameScope()` then `BattleUIService.OnGameStart()`.
- Euterpe's `MpGameStartBarrier` is a `Priority.First` **prefix** on `PnlBattle.GameStart` that
  returns `false` to freeze the player at frame 0, then re-invokes `_pnl.GameStart()` at the synced
  start (t0). Harmony **runs postfixes even when a prefix returns `false`**, so:
  - Call 1 (frozen, original suppressed): Info+ postfix still runs → creates **GameScope #1**
    prematurely, builds 6 text GameObjects.
  - Call 2 (t0, original runs): Info+ postfix runs again → `CreateGameScope()` disposes #1 and
    creates **GameScope #2**.
- `#1` disposal calls `Object.Destroy()` on its 6 text objects. Unity **defers destroy to end of
  frame**, so they remain findable via `Transform.Find()`. `#2`'s `CreateTextObj` does
  `parent.Find(name)` and **reuses the about-to-be-destroyed objects**. End of frame: Unity destroys
  them → `#2`'s references are dangling → `UpdateAllText()` throws every tick → `DesiredUiVisible`
  never initializes → `ForceHide()` forever → native battle panel stuck at `scale(1,3,1)` (invisible)
  and Info+ UI never appears.

Root causes: (a) postfix acts on a suppressed call; (b) scope churn disposes+recreates within a
frame; (c) reliance on `Transform.Find` to dedup objects across the deferred-destroy window;
(d) an exception in Info+'s own update loop silently breaks visibility init.

### 2.2 Over-engineering

A UI-overlay mod carries a full DI container (`SimpleServiceProvider`, property injection, static
injection, per-battle scopes). The scope lifecycle is bound to a Harmony postfix and re-injects
singletons on every create/dispose. This machinery *is* the fragility surface above and is not
justified by the mod's actual needs.

### 2.3 Scattered ecosystem assumptions

Info+ has **no** ecosystem-detection code. It reads metadata from game singletons (which both CAM
and Euterpe populate, so title/author/difficulty/level/album work for all three). The only divergent
concern is **personal best**:
- vanilla + **CAM** → native high-score store (`BattleHelper.GetCurrentMusicHighScore()`).
- **Euterpe** → its own save; the native store returns 0 → no PB shown. Fixed only on an unmerged
  branch via a reflection bridge.

"Where do I get this chart's PB?" is spread across `GameStatsService.Init`, `PreparationScreenService`,
`VictoryScreenService`, and the unmerged Euterpe bridge call. There is no single resolver.

## 3. Target architecture

Three layers, explicit composition, no DI container.

```
Presentation (Harmony patches)  ── thin, exception-isolated, forward to ──►  BattleController
                                                                                  │ owns
Global statics (process-life)                                              BattleSession? (per-battle)
  Config / Fonts / Log / Updates                                              ├─ BattleUi
  StatsStore / RuntimeData                                                    ├─ GameStats
                                                                              ├─ NoteTracker
ChartSource (static resolver) ◄── PB + ecosystem, queried by GameStats       └─ VictoryHandler
```

### 3.1 Global statics (process-lifetime)

The current "Global" services are genuinely process-global and have no reason to live in a container.
They become plain static classes (or one `Mod` facade holding plain instances), initialized once in
`MDIPMod.OnInitializeMelon` / `OnLateInitializeMelon`:

- `Config` — loads/saves/watches the YAML config modules; exposes typed accessors (`Config.Main`,
  `Config.TextFieldLowerLeft`, ...). Same file formats as today.
- `Fonts` — font asset loading/unloading (idempotent; already is).
- `Log` — logger.
- `Updates` — auto-updater (unchanged behaviour, just no injection).
- `StatsStore` — the persistent `info+_song_stats.json` cache (`StatsSaverService` today). **Same
  format, same key.**
- `RuntimeData` — in-session PB/intermediate cache (`RuntimeDataStore` today).

No `[Inject]`, no reflection, no scope. Construction order is explicit and visible.

### 3.2 BattleController (static coordinator)

The single entry point the patches forward to. Owns `BattleSession? _current`. Responsibilities:

- `OnGameStart(PnlBattle pnl, bool runOriginal)` — **the robust hook** (see §5): only acts when
  `runOriginal` is true; idempotent (a session already live for this battle ⇒ no-op).
- `OnSceneLoading()` — deterministic teardown of `_current` (replaces "Loading"-scene scope dispose).
- `OnFixedUpdateTick()` / `OnLateUpdateTick()` — forward to `_current` if alive.
- note/miss/result forwarders (`OnNoteResult`, `OnMissCube`, `OnSetVictoryDetail`, prep hooks).
- Every method is wrapped so an internal throw is logged and swallowed (defense in depth).

### 3.3 BattleSession (per-battle, plain object)

Constructed when a battle genuinely starts; disposed deterministically. Owns its sub-units as plain
fields (composition):

- `BattleUi` — builds/updates the overlay (the current `BattleUIService` logic): the 6 text fields,
  score-zoom follow, native-UI tweaks. **Tracks every native-UI side-effect** (AP icon position,
  pause-button size, panel scale) and restores them on `Dispose`. Owns its created GameObjects and
  destroys them on `Dispose`. Visibility (`DesiredUiVisible`) is initialized **eagerly** from config
  at construction, not lazily inside the zoom check.
- `GameStats` — score/accuracy/miss/combo tracking (`GameStatsService`). Reads PB via `ChartSource`.
- `NoteTracker` — note-event/record (`NoteEventService` + `NoteRecordService`).
- `VictoryHandler` — victory-screen detail + PB persistence (`VictoryScreenService`).

`Dispose` is idempotent and synchronous: restore native UI, destroy owned GameObjects, clear refs.
Because only one session exists per battle and it is never disposed-then-recreated within a frame,
the deferred-destroy reuse race cannot occur.

### 3.4 ChartSource (static ecosystem + PB resolver)

`ChartSource.ResolveCurrent()` → `{ Ecosystem (Vanilla|Cam|Euterpe), bool HasPb, int Score, float Accuracy }`.

- **Euterpe**: detect via reflection `Euterpe.EuterpeBridge.IsEuterpeUid(uid)` (do NOT hardcode the
  `2233-` prefix). PB via `Euterpe.EuterpeBridge.TryGetLocalBest(uid, difficulty, out score, out acc)`
  (acc is 0..1 per the pinned contract). Soft dependency: if the type/method is absent, fall through.
- **CAM**: detect via `MusicInfo.albumJsonIndex == 1000` (equivalently `uid` starts with `999-`).
  **PB path is identical to vanilla** (native store) — this is exactly today's behaviour, so CAM is
  not changed and cannot be broken. Detection is informational (labelling/extensibility).
- **Vanilla / unknown**: PB via the native store (`BattleHelper.GetCurrentMusicHighScore()`).

The reflection lookup is resolved once and cached (type + MethodInfo), guarded so a missing Euterpe
mod is silent.

### 3.5 Presentation (Harmony patches)

Patches stay as the thin outer layer; each **wraps its body in try/catch** and forwards to
`BattleController`. `PnlBattleGameStartPatch` postfix gains `bool __runOriginal`. No patch touches DI.

## 4. Song identity key — keep the content-derived hash

The persistent PB cache key stays `hash(difficulty + author + levelDesigner + musicName)`. **uid is
NOT used as a persistent key**: CAM's `999-{index}` is load-order-dependent and Euterpe's
`2233-{index}` is a per-client position id — both are unstable across sessions/clients. uid is used
**only at live query time** for the ecosystem PB source (`ChartSource`), where it is resolved within
the same session. Keeping the key unchanged also keeps the stats file migration-free.

## 5. Robustness rules (cross-cutting laws)

1. **`__runOriginal` guard.** A postfix acts only if the original method actually ran. When another
   mod suppresses `PnlBattle.GameStart` (MP freeze), Info+ does nothing on that call and builds its
   session only on the real start.
2. **Idempotent session.** `BattleController` no-ops if a live session already covers this battle;
   `BattleSession.Dispose` is safe to call twice.
3. **Patch exception isolation.** Every Harmony patch body and every `BattleController` entry catches,
   logs, and swallows. An Info+ fault never propagates into the game's call chain or other mods'
   patches. (Independently prevents the "native UI disappears" class.)
4. **Native-UI side-effect restoration.** Every native object Info+ mutates is recorded and restored
   on teardown, so repeated or out-of-order lifecycles never strand native UI.

## 6. Data & migration

- **Stats file** (`info+_song_stats.json`): format and key unchanged → no migration.
- **Config YAML modules**: schema preserved where possible. If any module's shape must change, a
  **one-shot, idempotent, guarded** migration runs on load (detect old shape → rewrite → mark done;
  never re-trigger, never partial-write that could corrupt). Per the approved Q1=(c): no user-facing
  data loss; per the constraint: migration must not introduce bugs or re-fire.

## 7. Change map

- **Delete**: `src/Application/DependencyInjection/*` (`SimpleServiceProvider`, `ModServiceConfigurator`,
  `PropertyInjectionExtensions`, `SimpleServiceProvider` scope types), all `[Inject]` usage, the
  `IXxxService` interfaces that existed only for the container.
- **Create**: `BattleController`, `BattleSession`, `ChartSource` (+ the Euterpe reflection adapter).
  Global statics holder(s).
- **Rewrite (logic preserved)**: the former scoped services become `BattleSession` sub-units; the
  former global services become statics.
- **Preserve**: config domain types & file formats, placeholder/text system, constants, UI layout,
  auto-updater behaviour, stats file format.
- **Supersede**: branches `claude/fix-gamestart-postfix-duplicate` and `feat/euterpe-pb-adapter` —
  their intent (the `__runOriginal` guard; the Euterpe PB bridge) is reimplemented cleanly here.

## 8. Work sequence (each step is a commit; build-verify between steps)

1. Branch + this design doc.
2. **Global statics**: replace the DI container with explicit static globals; wire `MDIPMod`. Compiles
   green on the new skeleton.
3. **ChartSource**: ecosystem detection + unified PB resolver + Euterpe reflection adapter.
4. **BattleSession + BattleController**: per-battle lifecycle replacing scoped services; apply the §5
   rules; native-UI restoration.
5. **Repoint patches** to `BattleController` (`__runOriginal`, try/catch); delete DI layer.
6. **Migration** (only if a format changed) + integrity sweep (no dead code, no stale comments).
7. **Verify**: Release build (verify-only) green; then K Debug-deploys and tests: vanilla solo,
   Euterpe solo PB display, Euterpe multiplayer (the bug case), CAM untouched.

## 9. Verification

No automated tests are feasible (Il2Cpp game runtime). Verification = `dotnet build -c Release`
(verify-only, no deploy) for compile correctness, then manual in-game testing by K via a Debug
build (which deploys to `Mods/`). The MP case is the acceptance test: enter an Euterpe multiplayer
round with Info+ enabled and confirm both native and Info+ UI render correctly.

## 10. Implementation notes (deviations from the plan above)

- **§8 sequencing.** The DI removal is atomic (services are coupled through injection; splitting it
  would require a forbidden shim), so the structural refactor landed as one coherent commit
  ("refactor: replace DI container with explicit composition"). `ChartSource` followed as its own
  commit because it depends on the `Log` static introduced by the refactor — so the order is
  structural-first, ecosystem-second (not ecosystem-first as §8 listed).
- **§5 rule 4 (native-UI side-effect restoration) was dropped — YAGNI.** The native objects Info+
  mutates (AP-icon position, pause-button size, panel scale) live in the GameMain battle scene, which
  the game destroys on every battle exit, taking them with it — there is nothing to restore on the
  normal path. The only mid-scene teardown is the game-update incompatibility shutdown
  (`FailAndShutdown`), where Info+ is already non-functional and a cosmetic native tweak is moot.
  Rules 1–3 — the ones that actually fix the bug (`__runOriginal` guard, idempotent session, patch
  exception isolation) plus the tracked-reference text reuse — are all in place.
- **§6 migration: no migration code was written.** Config schema and the stats key/format are
  unchanged, so the existing version-control path carries user values forward on a future version
  bump. `ModBuildInfo.Version` is intentionally NOT bumped here — that is a release-time decision for
  K, made when shipping via the auto-updater.
