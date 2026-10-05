# Voltix Dash — focused pre-WebGL optimization and QA

Completed 2026-10-05 using the UnityMCP server in Unity 6000.5.6f1. **Ready for the first WebGL build QA. No build was made, no packages were added, and nothing was committed.** Unity is back in Edit mode on the saved, unmodified-in-memory Level01 scene. Final Console: **0 errors**.

## What changed

Only two existing project files changed relative to the snapshot taken at the start of this pass:

- `Assets/Scripts/Player/PlayerCombat.cs`: replaced `Physics2D.OverlapBoxAll`'s per-strike result array with a reusable `List<Collider2D>`, initially sized for eight hits and allowed to grow. The query retains all-layer filtering and the current global trigger-query setting. It has no fixed hit cap. EnemyHealth detection, deduplication, reach, cooldown, damage, facing, accepted-attack events, and scoring are unchanged. Results are cleared after use so the list does not retain dead colliders.
- `Assets/Scenes/Level01.unity`: removed 15 confirmed redundant, disabled legacy SpriteRenderer components. Actual animated/visible child renderers remain. No GameObjects were deleted or moved.

The warm old overlap allocated **48 bytes per query** at spawn; 100 old queries allocated 4,800 bytes. The reusable overload allocated **0 bytes** across 100 warmed queries. Native timing for the isolated 100-query samples was 0.063 ms versus 0.030 ms; these tiny samples do not establish a meaningful game FPS improvement. Hit sets matched at all 48 collider centers with trigger queries both enabled and disabled (**96 comparisons**). Cold accepted-attack probes were 88 B before and 40 B after; nine warmed full accepted attacks in the post capture allocated 0 B. Cold initialization/event costs are not presented as steady allocations.

The removed renderer references included the obsolete `Player.png` and `PatrolBot.png`, which are now absent from Level01's dependency list. Their original art remains on disk for editing. Their combined Unity Editor-reported texture object sizes are **1,050,592 bytes**; the two 256×256 RGBA pixel payloads alone total **512 KiB**. Actual browser memory and compressed build savings need a build report. SpriteRenderer material references fell from four to two; no material assets or shaders were deleted.

**Asset/import settings changed: none. Assets deleted: none. Prefab assets changed: none. Player Settings and scene-build configuration changed: none.** MainMenu and Level01 remain enabled, in that order.

## Baseline and post measurements

The successful baseline preceded production changes. Both passes used native ProfilerRecorder counters, FrameTimingManager, raw GC.Alloc size metadata, the same controller-driven route, and the same damage, fall, pause, completion, reload and navigation probes. The driver supplied input/buffer state to the existing controller; it did not teleport the player during either full traversal or disable hazards. It reached all 32 platforms. Staged contact/range/fall tests were separate from traversal.

| Measurement | Before | After |
| --- | --- | --- |
| Approximate instrumented Editor loop FPS | 484 | 541 |
| Active CPU main-thread frame time, median / p95 | 0.386 / 0.647 ms | 0.332 / 0.466 ms |
| Main Thread including waits, median | 2.059 ms | 1.843 ms |
| Native GC allocations in steady behaviour callbacks, median | 0.0 B | 0 B |
| Whole-Editor GC counter, median | 14,835 B/frame | 14,835 B/frame |
| GC collections during traversal (generations 0 / 1 / 2) | 2 / 2 / 2 | 2 / 2 / 2 |
| Active GameObjects / components | 194 / 436 | 194 / 421 |
| Animators | 10 | 10 |
| SpriteRenderers, total / enabled | 105 / 90 | 90 / 90 |
| Shared SpriteRenderer materials | 4 (including disabled legacy references) | 2 |
| AudioSources / AudioListeners | 3 / 1 | 3 / 1 |
| Rigidbody2D / Collider2D | 7 / 48 | 7 / 48 |
| Whole-Editor loaded texture memory, median | 249.69 MiB | 250.70 MiB |
| Loaded AudioClip memory | 68,504 B (66.9 KiB) | 68,504 B (66.9 KiB) |
| Audio Used Memory, median | 1,424,880 B (1.36 MiB) | 1,424,880 B (1.36 MiB) |
| Standard draw calls, median / p95 / max | 8 / 16 / 22 | 8 / 16 / 22 |
| SetPass calls, median / p95 / max | 5 / 10 / 16 | 5 / 10 / 16 |
| Animators.Update, median | 0.037 ms | 0.033 ms |
| Spawn-to-goal driven traversal | 64.33 s | 64.22 s |
| Final score / kills / respawns | 400 / 4 / 0 | 400 / 4 / 0 |

These are **instrumented, uncapped macOS Editor measurements**, not browser/WebGL FPS. Early background Game-view rendering was throttled to about 10 FPS; captures from that setup were excluded from the full-run comparison. Main-thread active work is reported separately from waits. Editor views, MCP execution and raw-frame collection affect the global allocation counter, FPS, texture memory and GC collections. The behaviour-callback median is 0 B; event-driven HUD strings and other cold paths still allocate. The generation counts describe two whole-Editor collections reported in each generation, not six independent game collections.

The post run finished with 2 HP and the baseline with 1 HP, reflecting the timing of moving hazards in the automated route. Both traversals took damage, killed all four enemies, completed with score 400, and never respawned. These runs were QA traversals, not claimed damage-free speedruns. All **297 protected serialized Transform, Collider2D, Rigidbody2D and MonoBehaviour records match exactly** before/after. All other project files under Assets, Packages and ProjectSettings match the initial hashes.

Texture memory increased slightly in the whole-Editor counter because warm animation sheets and retained Editor assets remain loaded. It is not a claim of increased browser footprint or proof of runtime memory savings. Initial baseline scene-referenced loaded art was 22.23 MiB with only some player sheets warmed; post warmed art was 26.49 MiB. Those unequal warm-up states are intentionally not used as a before/after memory comparison. The removed-reference savings above compare the same two unchanged texture assets.

The Frame Debugger exposed six Main Camera render events at spawn: three sprite render groups, final blit, and UI overlay/draw ranges. Grouped render events are not equivalent to individual draw calls; native draw-call counters are in the table. Counts include Editor rendering and should be checked again in the browser.

## Load/reload and spike inspection

Measured request-to-sceneLoaded times, including frame scheduling and Editor overhead:

| Transition | Before | After |
| --- | --- | --- |
| Level01 probe reload | 142.6 ms | 163.4 ms |
| Completion → Restart | 295.9 ms | 179.8 ms |
| Pause → MainMenu | 136.7 ms | 150.0 ms |
| MainMenu → Play | 160.7 ms | 147.0 ms |

These are single transition samples, not a claimed loading speedup. Before-run event durations were measured with Stopwatch; post native event scopes also recorded allocation metadata. Warm empty attacks were about 0.04 ms in both captures. Before enemy-kill attacks were 0.33–0.41 ms. Post first kill was 1.85 ms / 636 B including cold presentation initialization; later kills were 0.23–0.32 ms / 256 B. Damage and lethal respawn probes remained below 1 ms. Cold post pause was 2.98 ms / 4,388 B; repeated mid-level pause/resume scopes allocated 0 B and took 0.04–0.08 ms. Completion Restart's UI callback was 5.03 ms before and 6.14 ms after, followed by the load times above. No repeatable gameplay spike justified another change. Baseline active CPU frame maximum was 4.37 ms; post was 3.44 ms. FrameTiming includes larger Editor/presentation outliers and is retained in the measurement JSON.

## Audit findings deliberately left unchanged

- **C# hot paths:** no LINQ, temporary collections, scene searches, delegate churn, or repeated component discovery inside gameplay Update/FixedUpdate/LateUpdate. Physics/render references are cached. UI string formatting happens on health/score/completion changes. Struct creation such as Vector2 and ParticleSystem.EmitParams is not a managed allocation. Cold component lookups in contact, completion and reload handlers remain readable and do not warrant caching/refactors.
- **Ground checks and Animator setters:** several presentation/controller ground checks and repeated Animator parameter writes exist, but measured costs are small. PlayerAnimationController.LateUpdate's median was approximately 0.0013 ms before; Animators.Update was approximately 0.037 ms. Combining those systems would change architecture for no demonstrated problem, so they remain.
- **Physics:** one dynamic player and six kinematic bodies (four bots/two movers), Z rotation frozen; static platform geometry stays static. All 48 collider shapes, transforms, triggers, layers and masks are unchanged. Queries and broad-phase costs did not justify collision-layer changes or disabling CameraBounds' collider.
- **Animation:** ten live Animators reuse five controllers and thirteen clips. Every clip has exactly one sprite reference track, and no animated float/transform/collider properties. Crisp transitions and the existing additive squash/stretch/flash remain unchanged. No duplicated controller/clip was found that warranted consolidation.
- **Rendering:** visible sprites use the shared NeonSprite and PlayerFlash materials; the attack uses its existing shared ElectricSlash material. Flash uses a MaterialPropertyBlock. No runtime material cloning or high draw-call count was found. No atlas, sorting, shader stripping or overdraw change was made without build/device evidence.
- **Textures:** all 24 PNG imports were reviewed, including 13 animation sheets. Mipmaps and Read/Write are already off, meshes use Full Rect, and generated physics shapes are unnecessary/disabled. The widest sheet is 2048 pixels. Uncompressed neon edges remain intact; reducing dimensions or changing compression before browser visual QA was not justified. Editable SVGs and offline Python art sources are preserved and are not runtime scene dependencies. There is no Resources/StreamingAssets inclusion of those authoring sources.
- **Audio:** six original short mono 22,050 Hz clips, preloaded PCM / DecompressOnLoad, about 67 KiB of decoded clip memory. Three bounded scene-owned sources already handle priority/cooldowns. Streaming, background loading, compressed decoding and more pooling would add complexity to tiny clips. Gains, cooldowns and listener behavior are untouched.
- **UI Toolkit:** HUD/completion/pause use cached element references and event updates. MainMenu's focus scheduling happens at enable time. No per-frame hierarchy queries/layout rebuilding or recurring UI construction was found. Existing hierarchy/style depth is small.
- **Object lifetime:** the attack line and death particles reuse their existing storage/system. Four short enemy death presentation instances per complete run are small; a generalized pool or enemy-lifetime rewrite is unjustified. Death presentation cleaned itself up, with exactly one score award.
- **Scene/build assets:** disabled renderers were removed only after checking that nothing serialized referred to them, their colliders had autoTiling off, and PatrolEnemy's optional legacy renderer lookup was guarded. The disabled animation-era renderers that were removed were proven redundant; actual animation targets remain. No further clearly unused referenced asset/component justified removal. Old source/prefab art references that are not live scene rendering were retained rather than deleting useful authoring assets or guessing at player-build dependencies.

## Verification

- Successful complete traversal before and after; all 32 platforms reached, four kills, score 400, no traversal deaths; attacks, hazards and final portal used their existing gameplay logic.
- J while moving right and Left Shift while stationary facing left both delivered accepted attacks through PlayerInput Send Messages. Both attack visuals faced correctly.
- Outside-range enemy survived; inside-range enemy died. Contact reduced player HP to 2/3 while the bot retained its 1 HP. Death visual appeared, then cleaned up, and score remained exactly 100 with one Killed event.
- All eight player states observed: Idle, Run, Attack, JumpStart, Rise, Fall, Land, Hurt. Bot facing matched patrol; six bot frames and six frames per barrier/mover instance observed. Animation source/controller files and all presentation timings are unchanged. All ten Animator progresses froze in the full-run pause checks, including the portal.
- HP HUD changed immediately to `HP: 2/3`; actual below-threshold falling respawn restored `HP: 3/3` and incremented deaths once. Lethal three-hit respawn restored full HP in both profiling passes. Score HUD became `SCORE: 0100`.
- Jump, landing, attack, damage and enemy death cues observed during focused QA. A real portal-trigger completion separately played the Completion clip; all six cues were checked. Audio sample positions froze while paused, resumed correctly, and reloading left three sources/one listener.
- Escape pause/resume, repeated full-run pause/resume, pause attack blocking, pause Restart, pause MainMenu, MainMenu Play, completion after earlier pauses, completion's Escape guard, and completion Restart passed.
- Completion displayed the correct score/death count, timer stopped, and controls/body simulation stayed blocked. Restart restored HP 3, score 0, deaths 0, four bots, ten Animators, three sources, one listener and one UIDocument.
- Compile succeeded. Final Console has 0 errors. The temporary input-focus override, test-driver PlayerLoop, background playback option, Editor pause and profiler settings were restored. Invalid fixture/throttling captures were excluded; they are temporary diagnostics, not baseline evidence.

## Exact scene component removals

- `Player` — removed its disabled legacy `SpriteRenderer` only.
- `Environment/Platforms/Platform_01_Opening` — removed its disabled legacy `SpriteRenderer` only.
- `Environment/Platforms/Platform_02_EasyJump` — removed its disabled legacy `SpriteRenderer` only.
- `Environment/Platforms/Platform_03_EasyJump` — removed its disabled legacy `SpriteRenderer` only.
- `Environment/Platforms/Platform_04_SpikeIntro` — removed its disabled legacy `SpriteRenderer` only.
- `Environment/Platforms/Platform_05_BotIntro` — removed its disabled legacy `SpriteRenderer` only.
- `Environment/Platforms/Platform_06_Elevated` — removed its disabled legacy `SpriteRenderer` only.
- `Environment/Platforms/Platform_07_Elevated` — removed its disabled legacy `SpriteRenderer` only.
- `Environment/Platforms/Platform_08_Elevated` — removed its disabled legacy `SpriteRenderer` only.
- `Enemies/PatrolBot` — removed its disabled legacy `SpriteRenderer` only.
- `Enemies/PatrolBot_02` — removed its disabled legacy `SpriteRenderer` only.
- `Enemies/PatrolBot_03` — removed its disabled legacy `SpriteRenderer` only.
- `Enemies/PatrolBot_04` — removed its disabled legacy `SpriteRenderer` only.
- `Hazards/MovingHorizontalHazard/HazardCore` — removed its disabled legacy `SpriteRenderer` only.
- `Hazards/MovingHorizontalHazard_02/HazardCore` — removed its disabled legacy `SpriteRenderer` only.

The two HazardCore removals are prefab-instance component overrides; their prefab files are untouched. All GameObjects, parenting, positions, scales, collisions and live visual children remain. No gameplay object was added.

## Files and evidence

Production changes: `Assets/Scripts/Player/PlayerCombat.cs` and `Assets/Scenes/Level01.unity` only. No `.meta`, import, prefab, material, animation, audio, package, UI, input or ProjectSettings files changed during this pass. Pre-existing workspace changes were preserved.

Created durable QA artifacts: `Reports/WebGLOptimizationQA.md` (this report) and `Reports/WebGLOptimizationMeasurements.json` (counter summaries, native overlap comparison, event/load measurements, QA results, inventories, renderer audit and file hashes). Temporary driver source/captures are confined to `Temp/CodexOptimizationChecks`, with the initial scene/hash snapshots in `/tmp`; they are outside Assets and cannot ship in the build.

## WebGL review and remaining build QA

No project gameplay code depends on threads, runtime filesystem writes, blocking external operations, native-only plugins or Editor APIs. Offline art/audio generators are authoring tools. Scene loading uses build-included paths. Quit is guarded out in WebGL/editor. Audio is bounded and driven by gameplay events. No compatibility issue warranted a Player Settings change.

Current settings use **Brotli**, no decompression fallback, and no WebGL threads. The hosting server will need appropriate compressed response headers and MIME types; confirm them on the first browser build. Initial WebGL memory is 32 MiB with growth enabled up to 2048 MiB; no actual browser peak was measured, so those settings were left alone. Editor quality is Very Low while the WebGL default is High: browser timings/quality must be measured at the intended resolution. No WebGL target switch or build occurred.

The remaining likely costs to investigate in build QA are transparent sprite/background fill rate at high device-pixel ratios, uncompressed animation/portal/background texture residency, initial scene/texture upload, and URP/UI/shader build footprint. None is established as a current bottleneck. Measure browser FPS, wasm heap/GPU memory, downloaded Brotli size, first-play audio unlock, scene reload pauses and low-end device behavior before making further optimizations. This pass is ready to proceed to that QA; it is not a browser performance certification.
