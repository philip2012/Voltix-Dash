# Reactor Heart artwork

LaunchPad.svg, Checkpoint.svg, SentryTurret.svg and SentryProjectile.svg are original editable geometric designs. Their antialiased PNG counterparts under Assets/Art/Sprites are the runtime sprites. `generate_reactor_art.py` uses the established geometric drawing helper without regenerating existing artwork; it requires Pillow in the authoring environment, not in Unity or the build.

Cyan indicates a safe launch/checkpoint; magenta, red and white distinguish sentries and projectiles. All use the existing NeonSprite material. Reactor structures, conduits and machinery guides in Level03 are scene-only SpriteRenderer/LineRenderer geometry without colliders or scripts.

The four Animator Controllers and nine small clips under Assets/Art/Animation/Reactor animate presentation only. Launch activation, checkpoint state, turret fire/death and projectile collision logic never depend on animation timing.
