# Level03 — Reactor Heart

Level03 is saved as the final campaign level. The intended competent clean run is **110–130 seconds**. The complete clean-ish test took **123.74 seconds**, finished with **2/3 HP, 0 deaths and 1,100 points**, and used the actual controller, physics, accepted attacks, hazards and portal. A second complete run with a deliberate natural pit fall after the checkpoint took **130.70 seconds**, finished with **2/3 HP, 1 death and 1,100 points**.

Unity is back in **Edit mode**, with **0 Console errors and 0 warnings** at final inspection. No commits, packages, Player Settings changes or WebGL rebuild were made.

## Counts

| Item | Count |
|---|---:|
| Platforms / traversal surfaces, including optional cover and recovery decks | 64 |
| MovingPlatforms, included in surface count | 7 |
| LaunchPads, including the introduction recovery pad | 9 |
| PatrolBots, 1 HP / 100 points each | 5 |
| SentryTurrets, 1 HP / 150 points each | 4 |
| Maximum available score | 1,100 |
| Environmental hazards | 9: 4 spikes, 3 barriers, 2 moving hazards |
| Midpoint checkpoints | 1 |
| Scene AudioSources / AudioListeners | 3 / 1 |

## Layout and pacing

The route spans x=0–832.4, climbs from floor y=0 to y=50, descends to y=36.5, then rises to the safe exit deck at y=49.6. The normal controller remains at serialized speed 8, jump force 14, acceleration 60, deceleration 70, coyote/buffer 0.12, attack reach 1.4/cooldown 0.35, 3 HP and 0.75-second damage cooldown. Camera follow tuning is unchanged. CameraBounds covers x=0–832.4 and y=−5–60; the dark background extends beyond the camera rectangle.

| Section | Surfaces | Design |
|---|---:|---|
| Fast reactor entry | 4 | Generous opening jumps, one bot then spikes; spawn is safe. |
| Launch introduction | 4 | Three clearly marked launch steps gaining 6 units each, followed by a wide landing deck. No landing hazard. |
| Moving machinery introduction | 6 | Horizontal ferry, safe landing, vertical lift and exit steps. A lower catch deck and recovery pad rescue missed first transfers. No sentry here. |
| First sentry | 2 | Wide deck and raised cover; learn the readable charge and dodgeable shot before closing to melee. |
| Reactor ascent | 12 | Launch up/right, reverse up/left, lift back up/right, bot, sentry, raised barrier approach, high launch and summit catwalks. |
| Midpoint checkpoint | 1 | A safe 20-unit deck. Marker at (323.2, 50.1) becomes the respawn destination. No adjacent enemy or overlapping hazard. |
| Core crossing | 7 | Two horizontal ferries around large magenta reactor structures; sentry on the intervening island with cover. Read the cycle, cross decisively, or use cover and approach to melee. |
| Descent / overload | 11 | Visible staggered landings, moving hazard, spikes, raised barrier approach and a lateral/upward redirect pad. |
| Security chamber | 1 | Wide 36-unit deck with two spaced bots and one sentry; enough space to dodge without environmental damage stacked into combat. |
| Final reactor run | 14 | Moving ferry into spikes, launch ascent, vertical lift, barrier, bot, moving hazard and final spike jump, with safe intermediate decks. The chamber sentry can swivel to cover the first final deck if left alive. |
| Final goal | 2 | Safe approach and 20-unit exit deck; portal at (825.4, 51.6), well beyond the last danger. |

The clean-ish run entered these sections at approximately 0 / 8 / 19 / 29 / 32 / 54 / 57 / 70 / 89 / 95 / 121 seconds. The checkpoint activates just before the second half. Moving platforms use 0.2-second horizontal endpoint pauses and 0.55-second vertical endpoint pauses; individual travel legs are short.

## New mechanics

**LaunchPad:** serialized upward velocity and optional horizontal velocity; applies an immediate Rigidbody2D velocity impulse. Landing/contact activation is latched until exit, supports repeated landings, pulses its presentation and raises a centralized audio event. It never changes controller tuning or gravity. Scene pads use upward velocities 20–26; the redirect and recovery pads add horizontal velocity 7.

**MovingPlatform:** simple two-point offsets from its starting world position, configurable speed and endpoint pause. Kinematic MovePosition, frozen Z rotation and interpolation; a zero-friction material avoids duplicate horizontal friction movement. Registered resting riders receive only deck displacement. The new platform's early presentation/controller integration removes its imparted resting normal velocity before the unchanged controller updates, allowing the ordinary grounded/coyote jump. Actual jumping removes the rider. Only these new platform tops use a one-way effector so recovery launches can pass upward through them; existing static colliders are unchanged.

**SentryTurret / EnergyProjectile:** cached player/camera, configurable 7.5-unit range, front/vertical/line-of-sight and viewport checks, 1.5-second cadence and 0.35-second charge visual. Only the chamber sentry is configured to swivel toward the player. Projectiles travel at 7 units/sec, last at most 2.5 scaled seconds, use bounded physics-cast buffers and deal 1 damage through PlayerHealth. Hit, environment collision or lifetime ends them. EnemyHealth remains authoritative for 1 HP, melee death and exactly one 150-point award. Turret body contact does no damage. Death presentation has no health/scoring component. No pool or per-turret AudioSource was added; cadence/lifetime naturally bound projectile counts.

**Checkpoint:** scene-owned, activates once, sets PlayerHealth's respawn destination and changes its visual/audio state. Lethal damage and falling still use existing full-HP respawn and death events. Score and killed enemies remain as before on ordinary death. Restart and scene reload recreate the original destination and inactive checkpoint. Levels01/02 have no checkpoint and retain their original starting respawn.

## Scene organization and preservation

Level03 was duplicated from Level02 to retain Player, HUD, managers, feedback, audio, camera and completion wiring, then its gameplay space was rebuilt. All 64 surface objects, 9 enemy roots, 9 hazard roots, 9 pad roots and the checkpoint are in the authoritative Layout.json and SceneAudit.json inventories. The copied Level02 gameplay layout/decorations were removed only from the new Level03 copy.

Roots are Main Camera, Global Light 2D, Player, Environment, Enemies, Hazards, LevelObjects and CameraBounds. Environment contains Platforms and Decoration; LevelObjects contains the existing GameManager/HUD/GameplayAudio/GameplayFeedback/GoalExitPortalVisual plus TraversalMechanisms and Checkpoint_Midpoint. Decoration uses existing body tiles and shared material for pylons, conduits, reactor rings and movement guides, plus 14 small section/readability text cues. It contains **0 colliders and 0 scripts**.

The final scene audit finds **405 GameObjects, 875 components and 0 missing scripts**. All six new prefabs also have 0 missing scripts and 0 AudioSources. Intentionally disabled legacy player/enemy renderers from the existing animation setup remain intact.

Level01 and MainMenu scene files are byte-for-byte unchanged. Level02 has exactly one changed field: nextLevelScene points to Level03. Existing PlayerMovement, CameraFollow, PlayerCombat, PatrolEnemy, EnemyHealth, ScoreManager, HazardDamage, feedback, animation controllers, six original SFX and physics settings were not edited. Serialized movement/combat/health/camera values match all three levels.

## Progression and UI

Build order is **0 MainMenu, 1 Level01, 2 Level02, 3 Level03**, all enabled. Level02.nextLevelScene is Assets/Scenes/Level03.unity. Level03.nextLevelScene is empty. LevelManager has a generic serialized completionTitle with LEVEL COMPLETE as default/fallback; LevelCompletionUI reads it. Level03 uses VOLTIX DASH COMPLETE and hides NEXT LEVEL. The existing label now has a query name and its text wraps in the existing panel, fixing the long final title without scene-name checks. Restart/Main Menu navigation and timer/score/death behavior are preserved.

## Created scripts and reusable prefabs

New scripts:
- Assets/Scripts/LevelObjects/LaunchPad.cs
- Assets/Scripts/LevelObjects/MovingPlatform.cs
- Assets/Scripts/Enemies/SentryTurret.cs
- Assets/Scripts/Enemies/EnergyProjectile.cs
- Assets/Scripts/Animation/SentryPresentation.cs
- Assets/Scripts/Game/Checkpoint.cs

New prefabs:
- Assets/Prefabs/LevelObjects/LaunchPad.prefab
- Assets/Prefabs/LevelObjects/MovingPlatform.prefab
- Assets/Prefabs/LevelObjects/Checkpoint.prefab
- Assets/Prefabs/Enemies/SentryTurret.prefab
- Assets/Prefabs/Enemies/EnergyProjectile.prefab
- Assets/Prefabs/Enemies/SentryDeathVisual.prefab

Assets/Physics/MovingPlatformSurface.physicsMaterial2D is the new zero-friction/bounce material for the new moving platforms.

## Art, animation and audio

Four original editable SVG designs and their antialiased PNG runtime sprites: LaunchPad, Checkpoint, SentryTurret, SentryProjectile. Sources live under Assets/Art/Source and PNGs under Assets/Art/Sprites. Source generator/documentation are generate_reactor_art.py and REACTOR_ART.md. No old artwork was regenerated. All reuse NeonSprite.mat.

Four controllers under Assets/Art/Animation/Reactor:
- LaunchPad.controller
- Checkpoint.controller
- Sentry.controller
- EnergyProjectile.controller

Nine clips:
- LaunchPad_Idle.anim, LaunchPad_Activation.anim
- Checkpoint_Inactive.anim, Checkpoint_Active.anim
- Sentry_Idle.anim, Sentry_Charge.anim, Sentry_Fire.anim, Sentry_Death.anim
- EnergyProjectile_Pulse.anim

Clips animate only SpriteRenderer color and, for the separate death visual, its local scale. They never animate gameplay colliders, platform paths or authoritative damage/fire timing. Existing squash/stretch, hit flash, death burst, camera shake, player/bot/environment animations and jump/landing audio notifications were retained.

Three original synthesized mono 22,050 Hz / 16-bit PCM cues under Assets/Audio/SFX: Launch.wav (0.23 s), Checkpoint.wav (0.35 s), TurretFire.wav (0.13 s). Source generator/documentation are Assets/Audio/Source/generate_reactor_sfx.py and REACTOR_AUDIO.md. The existing three-voice GameplayAudio subscribes to the new events. Old gains/cooldowns/cues are unchanged. New cues have their own short cooldowns and modest gains.

The four new PNG source files total **31,921 bytes** and three WAVs **31,442 bytes**; these are disk-source sizes, not measured WebGL download or GPU-memory sizes. New sprites are <=256 pixels, mipmaps/read-write disabled. Editable SVG/audio generator sources are not referenced by scene runtime dependencies. No new textures are needed for reactor decoration.

## Verification

| Test | Result |
|---|---|
| Normal and repeated pad launches, directional control | Pass; three activations, normal peak body y≈9.98 from first pad, player steers onto destination. |
| Pad pause/resume | Pass; position and upward momentum remain unchanged while paused, resume continues launch. |
| Pad respawn / subsequent reuse | Pass; latch clears, original spawn restored, second launch vy=25. |
| Stationary horizontal carry | Pass; 3.81 units deck travel with 0 relative drift in final fixture. |
| Walking while riding / landing on moving deck | Pass; relative walking displacement≈1.94 units; landing settles at deck top + player offset. |
| Ordinary jump-off / jump from rising lift | Pass; normal rise≈2.36 units; separate rising-lift test confirms accepted jump. |
| Vertical carry | Pass; separate fixture maximum resting height error≈0.034 units. |
| Moving-platform pause/resume / respawn | Pass; paused displacement0; resumes; rider count1→0 after respawn with no stale carry. |
| Intro recovery deck | Pass; recovery launch passes through one-way deck and rejoins MovingLanding with 3 HP. |
| Sentry front detection, rear/range/cover/off-screen rejection | Pass; front fires; all rejection fixtures produce0 shots. |
| Fire cadence / damage / dodgeability | Pass; three shots spaced≈1.50 sec, damage/lethal respawn via PlayerHealth; a normal jump dodges a shot with full HP. |
| Sentry melee kill / score once / body contact | Pass; accepted melee removes sentry, repeat hit still only150 points; body overlap leaves HP3. |
| Projectile movement, impact/lifetime cleanup, pause | Pass; speed7 within fixed-step sampling, player and environment cleanup, lifetime expiry, paused displacement0. |
| Checkpoint activation, lethal/fall respawn | Pass; one activation event, correct position and full HP; each respawn adds exactly one death. |
| Full clean-ish Level03 traversal | Pass;123.74 sec,1100 points,0 deaths,2 HP. |
| Full traversal with deliberate checkpoint pit fall | Pass;130.70 sec,1100 points,1 death,2 HP. Natural fall preserves500 points; respawn at323.2/50.1. |
| Completion / input lock / timer stop | Pass; final title,1100 score and correct deaths; accepted attack=false, body simulation stopped, elapsed time remains fixed, Escape does not create pause UI. |
| Completion Restart | Pass through actual UI; HP3,score0,deaths0,timer0,9 enemies and checkpoint inactive. |
| Pause / UI Resume / three repeated cycles | Pass; body, deck, Animator and timer deltas0; HP preserved, movement/attack blocked, audio paused; resume restores all. |
| Pause Restart after damaged active checkpoint | Pass through actual UI; reset to original spawn/full state/inactive checkpoint. |
| Pause MainMenu / MainMenu PLAY / campaign NEXT buttons | Pass through actual UI callbacks; MainMenu→Level01→Level02→Level03. Portal fixtures were used in Levels01/02, not full new traversals. |
| Level01/02 completion defaults / original respawn | Pass; LEVEL COMPLETE, NEXT visible, starting destination unchanged. Level03 has no NEXT. |
| Repeated scene loads / static event subscriptions | Pass; gameplay retains one manager/HUD/listener and3 sources; MainMenu has no gameplay manager/sources. Each new audio event has exactly1 subscriber after returning to Level03. |
| Edit-mode final state / missing scripts / Console | Pass; Level03 saved, Edit mode, missing scripts0, errors0, warnings0. |

Full traversals use a temporary in-memory test driver to issue controller movement/jump requests and accepted PlayerCombat attacks. They do not teleport or inject body velocity to cross the route. Independent mechanic/navigation fixtures place the player at their test setup locations; they are separate from traversal evidence. The intentional-death run walks into the checkpoint-adjacent pit and actually falls through the existing −40 threshold. Test code resides only in Reports or in-memory, and temporary PlayerLoop/delegate hooks are removed. Original Editor interaction/idle keys, GameView size/zoom and maximized state were restored.

## Iteration, compromises and remaining QA

Earlier passes exposed obstructed ascent clearance, launch-pad approach positioning and tight barrier approaches; the final scene fixes these through geometry and placement, without controller/hazard tuning changes. New moving-platform tests measured friction drift and lift normal-velocity interference with grounded jumping; fixes are isolated to the new platform/material. Aborted trial traces are retained. Some trial failures came from the temporary driver's lift waiting or Editor execution stalls; the final driver handles fresh-frame jump requests and the forward lift edge.

To keep the target at four sentries, the chamber sentry also provides optional overwatch of the first final deck rather than adding a fifth enemy. The complete route can be cleared without unavoidable damage; the clean-ish automated traversal took one avoidable moving-hazard hit near the end. No known scene/mechanic blocker remains from these Editor tests.

**A fresh WebGL build and browser regression pass for the new content is still required before claiming release readiness.** This task did not rebuild or retest the browser build; previous WebGL output still represents the earlier campaign content. Human feel/difficulty review is also useful alongside the recorded physics traversal, particularly launch steering and the signature core crossing.

## Exact changes and evidence

[All created/modified project files, including .meta files](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/FileInventory.md>)

Existing files modified by this task:

- [Assets/Scenes/Level02.unity](</Users/philips/Documents/UnityProjects/Voltix Dash/Assets/Scenes/Level02.unity>) — one next-level destination field only.
- [Assets/Scripts/Audio/GameplayAudio.cs](</Users/philips/Documents/UnityProjects/Voltix Dash/Assets/Scripts/Audio/GameplayAudio.cs>) — three new cues/events through existing three voices.
- [Assets/Scripts/Game/LevelManager.cs](</Users/philips/Documents/UnityProjects/Voltix Dash/Assets/Scripts/Game/LevelManager.cs>) — generic completion title.
- [Assets/Scripts/Player/PlayerHealth.cs](</Users/philips/Documents/UnityProjects/Voltix Dash/Assets/Scripts/Player/PlayerHealth.cs>) — respawn destination initialized from starting position plus minimal setter/read-only property.
- [Assets/Scripts/UI/LevelCompletionUI.cs](</Users/philips/Documents/UnityProjects/Voltix Dash/Assets/Scripts/UI/LevelCompletionUI.cs>) — query/display configured title.
- [Assets/UI/HUD/HUD.uss](</Users/philips/Documents/UnityProjects/Voltix Dash/Assets/UI/HUD/HUD.uss>) — title wrapping.
- [Assets/UI/HUD/HUD.uxml](</Users/philips/Documents/UnityProjects/Voltix Dash/Assets/UI/HUD/HUD.uxml>) — query name for existing title label.
- [ProjectSettings/EditorBuildSettings.asset](</Users/philips/Documents/UnityProjects/Voltix Dash/ProjectSettings/EditorBuildSettings.asset>) — append enabled Level03 at index3.

New scene: [Level03.unity](</Users/philips/Documents/UnityProjects/Voltix Dash/Assets/Scenes/Level03.unity>). **42 new non-metadata project files plus47 Unity metadata files**, no removed original project files. The pre-existing Voltix Dash.slnx modification and Reports/Level02 artifacts were preserved.

Evidence: [level map](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/LevelMap.svg>), [layout](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/Layout.json>), [scene object inventory](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/SceneAudit.json>), [tuning comparison](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/TuningComparison.json>), [clean traversal](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/FullRunH.json>), [checkpoint traversal](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/DeathRunC.json>), [checkpoint transition summary](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/CheckpointRunSummary.json>), [mechanic fixtures](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/MechanicQA.json>), [extended fixtures](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/ExtendedQA.json>), [rising jump](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/RisingJumpQA.json>), [safety fixtures](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/SafetyQA.json>), [pause fixtures](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/PauseQA.json>), [respawn fixtures](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/RespawnMechanicsQA.json>), [navigation/reload states](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/FlowRegression.jsonl>), [asset audit](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/AssetAudit.json>), [QA artifact list](</Users/philips/Documents/UnityProjects/Voltix Dash/Reports/Level03/ArtifactManifest.json>).
