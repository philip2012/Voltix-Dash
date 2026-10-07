# Stationary hazard damage fix — Voltix Dash

## Root cause and correction

The original HazardDamage called PlayerHealth.TakeDamage only from trigger/collision Enter and Stay callbacks. In Level01, StaticSpikes and ElectricBarrier are stationary trigger colliders without Rigidbody2D components. The player's body goes to sleep while standing on the supporting platform. Enter delivers the first hit, but Stay callbacks stop while the body sleeps, despite verified geometric overlap. The health cooldown expires normally; the missing hazard request is the fault. No collider configuration or PlayerHealth tuning bug was found.

HazardDamage now caches player contacts, validates real collider overlap in FixedUpdate, and requests damage only when PlayerHealth.IsInvulnerable is false. PlayerHealth remains the single cooldown authority for all simultaneous hazards. Solid contacts additionally require IsTouching and normal contact-offset separation. No new timer or damage value was introduced.

Exit/geometry separation prunes contacts. Disabled, destroyed, inactive or unsimulated colliders are rejected. OnDisable unsubscribes and clears contacts. DeathRespawned clears contacts immediately; direct teleports/manual respawn are also detected through geometry. Scaled physics updates stop while paused, so no catch-up damage is accumulated.

## Before and after characterization

| Hazard | Before | After |
| --- | --- | --- |
| StaticSpikes | Entry: 3→2. After >0.75 s, still 2 HP while sleeping, overlapped, and eligible for damage. Leave/re-enter: 2→1. Another entry caused lethal damage and respawn. | Continuous overlap: 3→2→1→0→3; accepted hit spacing ≈0.76 s. Leaving stops damage; re-entry works. |
| ElectricBarrier | Same stationary/sleeping failure as spikes; leaving/re-entry delivered another hit. | Same corrected continuous-overlap behavior and cooldown as spikes. |
| MovingHorizontalHazard | First hit works; the moving collider leaves and returns, producing later contact hits. There is no damage while physically separated. | Continues to damage during actual overlap, including retained contact; leaving/returning and lethal respawn work. |
| PatrolBot | Solid contact damage works while the bot is moving/contacting; it can move away, ending contact. Re-entry and lethal contact work. | Uses the same shared HazardDamage correction intentionally. Continuous real contact respects the same cooldown; HP/damage/patrol behavior remain unchanged. |

Raw timing, sleeping-body state, invulnerability state and signed collider-distance samples are in BeforeCharacterization.json and AfterCharacterization.json. Accepted hit gaps in the corrected Editor characterization are at least 0.75 s (approximately 0.76 s at the existing physics step).

## Editor regression results

All recorded assertions passed (EditorAssertions.json):

- Initial entry, stationary repeat hits, leaving, re-entry and lethal respawn for spikes and barrier.
- Moving hazard and PatrolBot contact damage, with unchanged global cooldown and amounts.
- Leave/re-entry test ends at 1 HP with zero retained contacts and no further damage outside the spikes.
- Pause maintains HP and scaled time; resume continues the remaining cooldown, then lethal respawn restores 3 HP.
- Disabling HazardDamage or its collider stops damage and prunes contacts; re-enable/re-entry works.
- Two coincident hazards produce only 3→2→1→0→3, rather than double damage, and both release stale respawn contacts.
- Destroying a contacting hazard leaves the player at 2 HP with no ongoing damage.
- Explicit respawn and falling below the existing threshold after spike contact restore HP and clear contacts; no later stale hit (FallRespawnRegression.json).
- Real portal trigger completes the level, Restart resets HP/deaths/state, MainMenu navigation and PLAY back to Level01 restore HP 3, score 0 and deaths 0.

Editor tests used in-memory UnityMCP C# probes, runtime-only player positioning and disposable runtime clones. No probe scripts or objects were saved under Assets.

## WebGL regression results

Non-development WebGL rebuilt successfully in Builds/WebGL: 187.8 seconds, 0 build errors, 1 existing compiler warning. MainMenu remains index 0 and Level01 index 1. Current Brotli configuration was preserved. The localhost server sent Content-Encoding: br and the correct application/wasm, application/javascript and application/octet-stream MIME types.

Chromium 154 (Codex in-app browser), actual rebuilt WebGL bundles, ordinary keyboard events only; no Unity runtime test bridge or production instrumentation:

- Canonical stationary spike test: BrowserMetrics-1791370351.json trace and BrowserMetrics-1791370355.json measurements. Three damage cues occur at 18442.0, 19191.9 and 19958.7 ms (gaps 0.750/0.767 s). Only one D-down and one D-up were sent. Rendered player position remains stationary in spikes, then returns to spawn with visible 3/3 HP.
- Canonical pause-overlap test: BrowserMetrics-1791370555.json trace and BrowserMetrics-1791370569.json measurements. Damage cues at 13142.0, 21908.5 and 22659.3 ms. Eight seconds of pause are included in the 8.767-second first gap; the next gap is 0.751 s. Movement was released before pausing and stayed released. No damage burst; player respawns at the original position at 3 HP.
- Initial QA-driver attempts with redundant movement edges or simultaneous movement-release/pause ordering were superseded by these canonical tests. They are retained as diagnostic artifacts and are not used as evidence of stationary behavior.
- A fresh complete traversal reached GoalPortal with SCORE 0400, HP 3/3, TIME 01:47.40, DEATHS 1. BrowserMetrics-1791370861.json records the completed route; BrowserMetrics-1791370903.json records exactly four actual enemy-death audio cues and one completion cue. The screenshot browser-proof-1791370902946.jpg shows the completion result. This was a regression traversal with one death, not a clean-run speed measurement.
- Completion ignores Escape and attack input. Completion Restart resets to spawn, HP 3/3 and score 0000. Pause → Resume, pause → Restart, pause → Main Menu, and PLAY again all passed via normal menu clicks. BrowserMetrics-1791370937.json and browser-proof-1791370937247.jpg record the final reload state.
- Animations, attack visuals, audio and hazards remained functional during the traversal. The browser maintained one AudioContext and the same 128,516,096-byte wasm heap through the menu/reload sequence; no visible duplicate HUD or audio behavior appeared. Internal Unity object counts were not exposed by the non-development browser build.
- Chromium console: 0 errors. One pre-existing unsupported URP FSR upscaling shader warning appears per fresh page load, including before this fix. It was deliberately left unchanged. See BrowserConsole.json.
- The earlier full-run attempt immediately following the paused probe was superseded by the fresh traversal; its QA movement prediction used wall-clock hazard phases across a pause. No production tuning was adjusted for the driver.

## Changed files and final state

Only production change: Assets/Scripts/Hazards/HazardDamage.cs.

No scene or prefab changes. SHA-256 comparison of all Assets, Packages and ProjectSettings files confirms no other project change. PlayerHealth, PlayerMovement, CameraFollow, all physics/collider/tuning values, damage cooldown 0.75 s, HP 3 and damage 1 are unchanged. Incidental Unity build serialization of four settings assets was restored to baseline (recorded in AutomaticBuildSerialization.diff).

New QA artifacts are under Reports/StationaryHazardFix; generated WebGL files are under Builds/WebGL. ArtifactManifest.md/ArtifactManifest.json list every file. Voltix Dash.slnx already differed before work began (InitialGitStatus.txt); that pre-existing change was preserved.

No packages added. No commit made. Unity is in Edit mode on Level01, scene not dirty, no compilation pending, and Unity Console contains 0 errors. Chromium Console contains 0 errors. The reported stationary-hazard bug is fully resolved in the verified Editor and rebuilt WebGL configurations.
