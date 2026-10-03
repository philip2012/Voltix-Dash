# Voltix Dash — first visual pass

Clean geometric sci-fi sprites: navy/charcoal structure, cyan energy, violet accents, orange/red danger. All assets are original code-drawn geometry with antialiased raster exports. No runtime vector renderer or added Unity package is required.

## Shared assets

- `Sprites/Player.png`: cyan armored runner.
- `Sprites/PatrolBot.png`: squat violet patrol chassis with warning bumper.
- `Sprites/PlatformBody.png`: repeating dark structural panels.
- `Sprites/PlatformSurface.png`: repeating cyan cap and tread strip.
- `Sprites/StaticSpike.png`: triangular orange blade.
- `Sprites/ElectricBarrier.png`: orange rails, violet field, cyan energy.
- `Sprites/MovingHazard.png`: warning core and armored frame, intended for the existing 45-degree visual rotation.
- `Sprites/DangerCrate.png`: warning skin for the legacy TestHazard.
- `Sprites/ElectricSlash.png`: translucent energy ribbon for the existing LineRenderer arc.
- `Sprites/GoalPortal.png`: cyan/violet exit frame; visual only, no goal logic.
- `Sprites/Background.png`: repeating low-contrast distant infrastructure.
- `Materials/NeonSprite.mat`: shared URP 2D unlit sprite material.
- `Materials/ElectricSlash.mat`: shared textured energy line material.
- `Prefabs/PlatformVisual.prefab`: reusable tiled body/surface, default width 4 units and height 1 unit.
- `Prefabs/Background.prefab`: reusable tiled backdrop.
- `Prefabs/GoalPortalVisual.prefab`: reusable exit decoration.

The existing gameplay hazard prefabs in `Assets/Prefabs/Hazards` reference this art. PNG sprites use full-rect meshes, bilinear filtering, no mipmaps, and no generated physics shape. Collider dimensions, physics bodies, gameplay scripts, and attack timing are unchanged.

## Dimensions and reuse

Actor and barrier PNG canvases are normalized to one unit, matching the existing nonuniform scene transforms. Their SVG sources keep readable aspect ratios. The spike pivot compensates for the original Unity triangle pivot; no scene geometry moved. PlatformVisual instances compensate for the original platform transform scale so the panel texture tiles instead of stretching across 50 units. Set the Body and Surface SpriteRenderer widths together when reusing it. The backdrop is behind gameplay on sorting order -100. The portal has no collider.

## Editable sources

`Source/*.svg` contains editable vector geometry. `Source/generate_art.py` regenerates all SVG and PNG exports with Pillow; it is an offline authoring utility and is not used by Unity or WebGL. Run it with a Python environment that already has Pillow. Unity uses the PNG files, not the SVG sources.
