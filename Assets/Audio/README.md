# Placeholder sound feedback

Six original procedural sounds for Voltix Dash, generated entirely from mathematical
oscillators and seeded noise. No third-party recordings, samples or music are used.
The WAVs and their generator may be shipped, modified and replaced without attribution.

Regenerate with `python3 Assets/Audio/Source/generate_placeholder_sfx.py`.
Files are mono, 22,050 Hz, 16-bit PCM. Durations: Jump 0.14s, Landing 0.09s,
Attack 0.14s, Damage 0.18s, EnemyDeath 0.22s, Completion 0.62s.
All clips peak at 0.55 before the quieter per-cue and master gains in GameplayAudio.

GameplayAudio uses three fixed, non-spatial sources on one scene-owned object.
Per-cue audio cooldowns suppress duplicates; important feedback can replace a lower
priority voice rather than queueing sounds. Completion clears other sounds for its
short chime. Restart destroys and recreates the scene-owned pool; there is no persistent
audio singleton and no additional AudioListener.

Jump/landing signals come from PlayerFeedback's observed motion, not input presses.
Attack, damage, enemy death and completion use the existing gameplay events.
The added PlayerFeedback notifications do not change physics or visual feedback.
