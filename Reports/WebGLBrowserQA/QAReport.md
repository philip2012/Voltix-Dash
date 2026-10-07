# Voltix Dash — first WebGL candidate QA

**Verdict: V1 CANDIDATE.** The non-development WebGL build succeeded and two complete browser traversals passed. No remaining browser-specific blocker was reproduced. Broader browser/hardware and subjective audio coverage remain outstanding; this is not a universal compatibility certification.

Build: `Builds/WebGL`. Local production URL: http://127.0.0.1:8765/ . Start the included server from the project root with `python3 Reports/WebGLBrowserQA/serve_webgl.py`. It binds localhost only. No packages were added and nothing was committed.

## 1. Build and configuration

Unity 6000.5.6f1, IL2CPP, **non-development**. Initial build: succeeded in 279.55 s. Rebuild after the page-sizing fix: succeeded in 39.88 s. Both reports contain 0 errors and 1 existing C# obsolete-API warning.

MainMenu remains enabled index **0**, Level01 enabled index **1**. Brotli, decompression fallback disabled, threads disabled, memory growth enabled all remain unchanged. No gameplay/scene/art/animation/audio/physics changes were made.

Only deliberate Player Settings change: `webGLTemplate: APPLICATION:Default` → `PROJECT:Voltix`, for the reproduced narrow-window clipping bug below. Incidental build-generated URP/preloaded-asset serialization was restored; `AutomaticBuildSerialization.diff` records that transient output. SHA comparisons in `ProjectChanges.json` confirm the final changes.

## 2. Browser actually tested and method

**Codex In-app Browser, Chromium 154.0.0.0, macOS**, WebGL 2. No Safari, Firefox, separate Chrome installation, mobile browser or real high-DPI display was tested.

Tested the actual built Unity bundles over HTTP. Full traversals used QA-only page instrumentation around the unchanged non-development bundles. The driver observed rendered player pixels/camera uniforms and dispatched ordinary movement/jump/attack keyboard events; it did not teleport the browser player, change HP/score, or invoke Unity gameplay methods. Trusted key events and mouse/menu actions were also tested. The uninstrumented production page was checked separately for loader, menu, gameplay, narrow/wide window sizing and console errors.

Instrumentation measures browser requestAnimationFrame cadence and Unity callback wall time. These are **not Unity Profiler main-thread timings or GPU timers**. Pixel observation adds overhead. Earlier fixed-time replay/Editor route experiments did not count as completed browser runs; preliminary metrics with heavier instrumentation are retained but excluded from the reported traversal measurements.

## 3. Build and download sizes

Final directory: **13,284,386 bytes (12.67 MiB)**. The main downloadable bundles plus loader total **13,264,377 bytes (12.65 MiB)**, excluding small HTML/template files and HTTP headers. Individual sizes:

| File | Served bytes | Decoded bytes |
|---|---:|---:|
| WebGL.wasm.br | 8,676,980 | 48,841,089 |
| WebGL.data.br | 4,488,450 | 12,381,518 |
| WebGL.framework.js.br | 71,964 | 406,315 |
| WebGL.loader.js | 26,983 | 26,983 |

`FinalBuildInventory.json` lists every generated build file, size and SHA-256. Compressed transfer sizes were independently verified using server responses and browser Resource Timing.

## 4. Loader, PLAY and scene reloads

The initial uninstrumented first visit took approximately **2.9 s** from engine startup logs to MainMenu initialization; that is an approximate observation, not a precise navigation-to-interactive measurement. Instrumented warm-process loads measured **0.26–0.44 s** to the Unity instance-ready callback. A HTTP/Unity-data-cache-bypassed load downloaded both complete Brotli bundles and measured **0.338 s** ready time on localhost. This still benefits from a warm browser process/compiler and must not be described as a clean-machine internet cold load.

First PLAY: **228 ms to the initial grounded-player landing cue**, with a 45.4 ms largest Unity callback around load. This is an upper bound to usable gameplay and includes initial spawn settling. Completion Restart: 178 ms / 8.9 ms callback. Pause Restart: 196 ms / 9.3 ms callback. MainMenu → PLAY again: 196 ms / 27.3 ms callback. No recurring scene reload stalls were observed.

## 5. Actual WebGL performance

A complete traversal measured over its 61.45 s moving/active window:

| Metric | Result |
|---|---:|
| Approximate FPS | 60.0 |
| Browser frame interval median / p95 / p99 | 16.7 / 17.6 / 17.7 ms |
| Worst interval in traversal | 17.8 ms |
| Rendered Unity callback median / p95 / max | 1.1 / 1.9 / 6.9 ms |
| Draw calls per rendered callback median / max | 10 / 15 |

Attacks, enemy kills, damage and completion produced no recurring stutter in this run. Startup callbacks reached about 80 ms, with initial frame intervals around 100 ms. Screenshot uploads and QA control work can cause additional outliers; they were excluded from the traversal window. Earlier heavily instrumented resize samples are not reliable production spike measurements.

A separate ~29 s **1920×1200 render-buffer test** within a 960×600 CSS canvas held ~60 FPS: interval p95 17.6 ms, max 17.8 ms; rendered callback median 2.1 ms, p95 2.9 ms, max 3.9 ms. This emulates doubled render density, not an actual OS DPR=2 display, and samples the opening/background rather than an entire second high-DPI traversal.

No CPU/GPU bottleneck was demonstrated on this machine. GPU duration/VRAM measurements were unavailable; asynchronous upload call times do not prove GPU upload cost. Measurements and windows are reproducible in `PerformanceAnalysis.json` and `analyze_metrics.py`.

## 6. Memory and remaining footprint risks

WebAssembly heap capacity: **107,085,824 bytes (102.13 MiB)** in MainMenu, growing to **128,516,096 bytes (122.56 MiB)** on first Level01. It stayed at that capacity through completion, multiple scene reloads, menu returns, respawns and doubled render density. This is allocated linear-memory capacity, not live object usage; no live wasm-heap or GC counters were exposed by the release build.

Observed JS heap after collection generally ~30–60 MiB; startup/reload/test snapshots were temporarily higher. QA metric arrays and screenshots contribute to JS memory, so these observations cannot establish a Unity JS leak.

The detailed build report attributes ~18.81 MB of packed assets to Texture2D, ~0.91 MB to Shader, ~0.52 MB to Cubemap, ~0.061 MB to AudioClip, ~0.033 MB to AnimationClip and ~0.0045 MB to AnimatorController. Packed asset accounting is not GPU residency. Largest artwork includes Background (~4.72 MB), GoalPortalLoop (~1.48 MB), PlayerRun (~1.05 MB), PlayerAttack (~0.66 MB); Unity splash logo contributes ~2.80 MB. Background/portal uploads and animation-sheet residency did not produce recurring traversal stalls or continued heap growth. Exact GPU texture residency and shader/UI sub-footprint were unavailable. No speculative stripping, compression, texture or URP changes were made.

## 7. Gameplay/menu/pause/completion results

**Complete required flow passed:** MainMenu → PLAY → complete → completion Restart → pause → Resume → pause → Restart → pause → Main Menu → PLAY again.

- A/D movement and facing, Space jumping, J and Left Shift attacks accepted. Player locomotion/air/hurt/attack animation, slash and squash/stretch remained visible. Patrol animations/facing and enemy death visuals worked.
- Static spikes, both moving hazards and electric barriers were encountered; HP updated immediately. Observed successive moving-contact damage cues were ~0.767 s apart, consistent with the existing 0.75 s cooldown. Deliberate lethal contact restored full HP at spawn; a separate fall restored full HP at spawn.
- Four enemies killed in each completed run. Score advanced in +100 increments and ended at **0400**, with no repeated awards. Restarts returned HP to 3/3, score to 0000 and enemies to their initial state.
- First completion: **TIME 01:06.45, DEATHS 0, SCORE 0400**. Second completion after deliberate lethal and fall deaths plus pause/resume: **TIME 02:25.57, DEATHS 2, SCORE 0400**. Timer remained fixed on the completion overlay; normal gameplay input/attacks were blocked and Escape did not create conflicting completion UI.
- Repeated pause/resume froze/restored gameplay and visual simulation; scene restart and MainMenu navigation worked after completion and after pause. QUIT was harmless in WebGL.
- No duplicated HUD/menu, repeated score/kill response, persistent scene state or additional WebAudio context was observed after repeated loads. Source inspection confirms scene-owned managers/audio and balanced event subscriptions. Exact Unity GameObject/AudioSource/AudioListener counts are not accessible from this non-development browser build, so this is behavioral and source-backed verification rather than a runtime object census.

## 8. WebGL presentation, input and audio

Initial loader completed; PLAY interaction focused the game and keyboard controls worked. Clicking outside and back into the canvas restored keyboard control. Space/J/Left Shift were not visibly intercepted while the canvas was focused. Escape toggled pause in windowed mode. Menu, HUD and completion layout remained readable at 1280×720 (16:9 viewport), 640×720 and 1600×720; separate 16:9, square and wide canvas-aspect tests also passed. Canvas resize adjusted rendering rather than stretching sprites; doubled render resolution remained readable.

Fullscreen entry worked with readable HUD. The automation-delivered Escape paused the game while the browser fullscreen presentation remained active; native browser fullscreen-exit interception could not be conclusively tested with this input surface. Reload restored windowed mode. Native hardware-key fullscreen exit needs a follow-up manual check; no production workaround was added.

AudioContext was suspended before interaction and entered running state immediately after the first gesture (about 15 ms in the measured PLAY flow). No gameplay cue exists in MainMenu before the gesture, so there was no queued startup cue to lose. All six gameplay cue triggers were observed with non-zero sample data and short, promptly started buffers. Unity briefly creates/stops a probe buffer before the actual playback buffer; those are not evidence of duplicate game AudioSources. Repeated loads retained one browser AudioContext. Pause blocks gameplay cues and normal cue behavior resumes; no delayed cue burst was observed. Subjective listening/mix quality was not verified through speakers and remains a manual check.

## 9. Browser console and serving

Final browser error queries: **0 errors**. One distinct warning: unsupported URP Edge Adaptive Spatial Upsampling shader / post-processing passes skipped. It occurs at page initialization. No visible rendering failure was found; Level01 does not enable camera post-processing. Kept unchanged because no demonstrated production problem justified altering URP.

Brotli serving **passed**: all three `.br` bundles returned HTTP 200, `Content-Encoding: br`, `Vary: Accept-Encoding`; wasm MIME `application/wasm`, data `application/octet-stream`, framework JS `application/javascript`. Loader JS was served uncompressed with JavaScript MIME. No decompression fallback was enabled. `FinalHTTPHeaders.json` records exact responses. The localhost QA server uses no-store for reproducible testing; public hosting will need equivalent MIME/encoding support with its own cache policy.

## 10. Bugs, fixes and remaining work

**Fixed browser production bug:** default fixed 960×600 desktop canvas exceeded a 640 px browser window (left=-160, right=800), clipping the HP HUD and score label. Cause: Unity default desktop template assigns fixed CSS dimensions. Added a custom template copied from the installed Unity default and a small resize/fullscreen-aware proportional fit function. At 640×720 after rebuild, canvas bounds are **left=12, right=628, width=616, height=385** and the full HUD is visible. Both complete browser traversals and menu/pause/restart flows passed on the rebuilt candidate. No mechanics changed.

**Additional existing behavior:** motionless player overlap with static spikes can stop subsequent damage once Rigidbody2D sleeps. Moving again resumes damage and caused the lethal hit. Reproduced independently in the Editor (HP 3→2, still inside spikes ~9.8 game seconds later, sleeping=true), so this is **not WebGL-specific**. Likely cause is sleeping-body stay-callback behavior. Left unchanged under the browser-only fix scope. `StationaryHazardObservation.json` contains evidence; address separately if sustained stationary contact is intended.

**Must-fix browser blockers before V1:** none reproduced after the responsive-page fix.

**Follow-up / nice-to-have:** stationary spike-contact behavior review; native fullscreen Escape check; speaker/listening check; Safari/Firefox and lower-powered/real high-DPI hardware coverage; hosted cold-network load testing; cosmetic URP warning/build-footprint review only if later measurements justify it. These are why the verdict is **V1 CANDIDATE**, not V1 READY.

## 11. Files, scene objects and artifacts

Modified existing project file: **ProjectSettings/ProjectSettings.asset**, template selection only. Created **Assets/WebGLTemplates/Voltix/index.html**, copied default template stylesheet/images/thumbnail, and Unity `.meta` files. `ProjectChanges.json` lists every exact created/modified path. No project file was deleted. **No scenes, scene objects, gameplay scripts, tuning, packages, art imports or build scene order changed.**

All generated build and QA files are enumerated with sizes in `ArtifactManifest.json`; `ArtifactManifest.md` gives the readable path list. QA files remain outside Assets and are not shipped. The two `capture-*.png` files are invalid black-canvas capture experiments and are explicitly not QA proof. Use the `browser-proof-*.jpg` screenshots. Editor route recordings are input-planning experiments, not browser performance/completion evidence.

Unity returned to **Edit mode**; final Console error query returned **0 entries**. No commit was made.
