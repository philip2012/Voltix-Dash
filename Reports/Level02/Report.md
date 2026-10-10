# Voltix Dash — Level02: Overcharge Sector

Level02 is saved at `Assets/Scenes/Level02.unity`. It was duplicated from Level01 to preserve its player, UI, completion, pause, audio and animation wiring, then its gameplay space was replaced with a new layout. Level01's only scene change is its serialized next-scene path. No gameplay tuning, packages, art assets, prefabs or Player Settings were changed. Nothing was committed.

## Result and timing

- Target: 80–100 seconds; designed near the upper end of that range.
- Final upper-route traversal: **99.24 seconds**, **500 score**, **3/3 HP**, **0 deaths**.
- Final lower-route traversal: **99.96 seconds** on the level timer, **600 score**, **2/3 HP**, **0 deaths**. One avoidable moving-hazard hit occurred during descent.
- **58 platforms**, **6 PatrolBots**, maximum **600 points** at the existing +100 each.
- **14 environmental hazards**: 4 spike placements, 5 barriers and 5 horizontal movers.

These are Editor Play-mode traversal measurements, not a new WebGL benchmark. Full routes used the real, unchanged movement controller, physics, melee combat, enemy health, damage, score and goal trigger. A temporary in-memory QA driver supplied movement/jump commands and invoked accepted attacks; it did not move the player by teleport or inject velocities during full traversals. Manual human play can produce different completion times.

## Layout and pacing

| Section | Platforms | Design |
|---|---:|---|
| Fast opening | 5 | Four generous gaps and varied heights establish immediate momentum; spawn is safe. |
| Vertical shaft | 13 | Twelve staggered shelves plus an exit deck climb from 2.55 to 21.8 units. Alternating left/right movement, varied shelf widths, one visible spike placement and one bot. |
| Moving hazard timing | 4 | Two movers on wider decks, separated by raised safe landings; timing rewards decisive movement. |
| Electric corridor | 5 | Three barrier decks alternate with higher safe shelves. Existing jump height clears barriers without mandatory damage. |
| Route choice | 13 | Marked upper coolant catwalk and lower overcurrent route reconnect on a shared deck. |
| Combat platform | 1 | A 34-unit arena with two bots spaced 14 units apart; the exit spike is clear of their patrol ranges. |
| Descent | 6 | Readable staggered drops of 1.6–1.8 units and one mover. Landing decks remain visible before committing. |
| Breather | 1 | A 14-unit safe deck with no enemy or hazard, framing the final sequence. |
| Final gauntlet | 8 | Varied gaps/heights combine two bots, spikes, a barrier and a mover, with safe intermediate landings. |
| Goal | 2 | Safe approach and 20-unit exit deck. Portal is well beyond the last hazard. |

The shaft shelves rise by 1.6–1.9 units, below the measured controller jump rise of approximately 2.34 units. Typical horizontal gaps are 2.2 units; shaft gaps are 1.8–2.2. Wide landing surfaces avoid precision requirements. Thin shaft shelves provide overhead clearance without changing player colliders or physics.

### Optional routes

The fork explicitly labels **JUMP / COOLANT CATWALK / UPPER / SAFER** and **DROP / OVERCURRENT / LOWER / +100**. Cyan and magenta structural guides reinforce the two paths using geometry and background decoration, not a separate camera.

Upper has seven shelves, no hazards and no bonus enemy. Lower has four decks, an electric barrier, a moving hazard and the optional PatrolBot. The lower path uses fewer jumps but greater hazard exposure. Both paths reconnect on a long shared deck before the combat arena. Upper-route players can complete the level without entering the lower route and earn 500 from the five required-route bots. The lower route makes all six bots accessible for 600.

Testing exposed a real headroom problem beneath the first catwalk layout. The upper catwalk and shared reconnection were raised and its final step reshaped. Both complete routes subsequently passed. The lower route has headroom for walking beneath the final upper shelf, followed by clear shared space before its next required jump.

### Enemies

| Object | Encounter | Speed | Patrol distance |
|---|---|---:|---:|
| PatrolBot_Shaft | Shaft_06 | 2 | 2 |
| PatrolBot_Optional | Lower_02 | 2 | 3 |
| PatrolBot_Arena_A | CombatDeck | 2 | 3 |
| PatrolBot_Arena_B | CombatDeck | 2 | 3 |
| PatrolBot_Final_A | Gauntlet_04 | 2 | 3 |
| PatrolBot_Final_B | Gauntlet_07 | 2 | 3 |

Only the shaft bot uses a shorter already-configurable patrol distance to fit its shelf. Existing speed, 1 HP, damage, animations and 100-point value are preserved.

## Visuals, camera and hierarchy

Existing neon sprites, materials, animation controllers and clips are reused. Violet platform bodies, a violet/navy background tint, reactor silhouettes, energy conduits and warning accents distinguish Overcharge Sector. Cyan surfaces continue to identify safe landings; danger remains red/orange. There are no new textures, sprite sheets, materials or editable artwork assets.

Scene-only decoration contains **0 colliders and 0 MonoBehaviour scripts**. It adds no Update loops. CameraFollow and its tuning are unchanged. CameraBounds spans X **0–697.8** and Y **−5–36**, supporting the shaft, upper branch, descent and final decks. The background covers those bounds; 16:9 beginning/end clamps and route/descent views were checked.

```text
Main Camera
Global Light 2D
Player
Environment
  Platforms (58)
  Decoration
    Background
    RouteCues (12 text labels)
    Reactor housings / shaft pylons
    Energy conduits / route guides / launch markers
Enemies (6)
Hazards (14)
LevelObjects
  GameManager
  HUD
  GoalExitPortalVisual
  GameplayFeedback
  GameplayAudio
CameraBounds
```

In Level02 only, the copied Level01 platforms, hazards, bots and tutorial cues were removed and replaced. Player moved to the new spawn, the existing portal to the new safe exit deck, Background was resized/tinted and reparented under Decoration, and CameraBounds was resized/repositioned and made a scene root. Existing managers, HUD, audio and feedback objects retain their wiring. The complete object/component/position/sprite/material inventory is in `SceneAudit.json`; `Layout.json` lists every new gameplay placement and route.

## Generic progression and modified production files

New files:

- `Assets/Scenes/Level02.unity`
- `Assets/Scenes/Level02.unity.meta`

Modified existing files:

- `Assets/Scripts/Game/LevelManager.cs`: optional serialized `nextLevelScene`, guarded `HasNextLevel` and `NextLevel()`; completion Restart loads the current scene through shared navigation.
- `Assets/Scripts/Game/SceneNavigation.cs`: generic `LoadLevel(scenePath)` resets playback state and loads that scene; MainMenu PLAY still starts Level01.
- `Assets/Scripts/Game/LevelPause.cs`: Restart reloads its current level instead of always Level01.
- `Assets/Scripts/UI/LevelCompletionUI.cs`: caches/subscribes the NEXT LEVEL button and displays it only when a valid next scene exists.
- `Assets/UI/HUD/HUD.uxml`: adds the initially hidden NEXT LEVEL button, reusing existing button styling.
- `Assets/Scenes/Level01.unity`: one added serialized line, `nextLevelScene: Assets/Scenes/Level02.unity`; no layout changes.
- `ProjectSettings/EditorBuildSettings.asset`: enabled Level02 appended at index 2. MainMenu remains 0 and Level01 remains 1.

Level02 leaves `nextLevelScene` empty, so its completion overlay has no NEXT LEVEL button. Restart works in either level; Main Menu remains available through the existing pause flow. A future Level03 needs only a build entry and Level02's next-scene field.

Baseline file hashes confirmed **exactly these seven existing project files changed** and no existing project assets were removed. PlayerMovement, CameraFollow, PlayerHealth, HazardDamage, combat, score, animations, audio, art imports, Packages and physics/Player Settings are unchanged. The existing `Voltix Dash.slnx` modification was present at task start and was not edited by this work.

## Verification

- Compilation succeeded; both final routes completed through the actual GoalTrigger.
- Upper: five kills/500 score; lower: six kills/600 score, no duplicate score.
- Pause/resume repeated three times; timer and HP remained stable while paused, audio paused, movement/combat restored on resume.
- Completion stops timer and gameplay; pause does not conflict after completion and attacks are rejected.
- Completion Restart and pause Restart reload Level02 with HP3, score0, deaths0, timer0 and all six bots restored.
- Pause Main Menu → PLAY loads Level01; Level01 completion Restart remains Level01.
- Level01 completion overlay visibly showed NEXT LEVEL; actual button callback loaded Level02 with fresh state. Level02 hides it.
- Stationary spike overlap damaged the player, stopped during pause, resumed to lethal damage/respawn and restored HP3. No stale hazard damage remained after respawn.
- A separate opening-pit fall respawned at the Level02 starting position, restored HP/HUD and incremented deaths; Restart cleared deaths.
- Separate descent fixture walked the drops, jumped the final equal-height gap and reached the breather in 10.69 seconds with HP3/deaths0.
- Repeated loads maintained one gameplay LevelManager, UIDocument and AudioListener and three existing centralized AudioSources. MainMenu had one listener/UI and zero gameplay managers/sources.
- Native GameView checks verified route signs, visible descent landings and completion overlay fit at 16:9.
- Final saved scene has **0 missing scripts and 0 Console errors**; Unity is in Edit mode.

Isolated hazard/fall/Level01 portal fixtures used runtime repositioning to exercise those cases; those are separate from the complete Level02 traversals. QA helpers live under Reports and are compiled in memory, not imported into Assets or shipped. Temporary Editor throttling and GameView settings were restored.

Earlier trial logs remain for traceability: some failed from background Editor test-driver throttling, one from a test-driver loop restore error, and one exposed the corrected route headroom problem. A first descent fixture incorrectly walked a same-height gap; the corrected fixture passed. Unity screenshot capture produced transient Metal memoryless-surface errors, so final visual checks used native UI screenshots. One Unity internal EditMode lifecycle diagnostic occurred when ending a QA session; it is recorded separately. These capture/Editor diagnostics were cleared, and a subsequent ordinary Play/Stop smoke check was used to verify the final Console state. No production workaround was added for tooling diagnostics.

No WebGL rebuild was performed for this level-design task. The existing browser build therefore does not yet contain Level02; browser QA of a new build remains a separate verification step.

## Artifacts

`LevelMap.svg` shows the complete layout. `UpperFinalV1.json`, `LowerFinalV5.json`, `DescentWalkFinal.json` and `FlowRegression.jsonl` contain the final measurements and state checks. `SceneAudit.json` and `Layout.json` provide full scene inventories. `ChangedProjectFiles.json` records the baseline hash comparison; `ArtifactManifest.json` lists every generated QA/report file. Helper C# and Python files are design/QA artifacts, not production scripts. Earlier helper snippets represent staged iterations; the saved scene, final Layout.json and SceneAudit.json are authoritative.
