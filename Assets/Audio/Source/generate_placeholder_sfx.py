"""Original, deterministic mono SFX; only Python's standard library is required."""
import math
from pathlib import Path
import random
import struct
import wave

RATE = 22050
OUTPUT = Path(__file__).resolve().parents[1] / "SFX"
OUTPUT.mkdir(parents=True, exist_ok=True)


def synth(name, duration, frequency, brightness=0.15, noise=0.0, decay=4.0):
    rng = random.Random(710)
    phase = filtered = 0.0
    samples = []
    count = round(RATE * duration)
    for i in range(count):
        t = i / RATE
        u = t / duration
        phase += 2 * math.pi * frequency(t, u) / RATE
        filtered += 0.2 * (rng.uniform(-1, 1) - filtered)
        tone = math.sin(phase) + brightness * math.sin(2 * phase)
        envelope = min(1, t / 0.003) * math.exp(-decay * u) * min(1, (duration - t) / 0.012)
        samples.append((tone + noise * filtered) * envelope)
    peak = max(abs(s) for s in samples)
    data = b"".join(struct.pack("<h", round(32767 * 0.55 * s / peak)) for s in samples)
    with wave.open(str(OUTPUT / f"{name}.wav"), "wb") as out:
        out.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
        out.writeframes(data)


if __name__ == "__main__":
    synth("Jump", 0.14, lambda t, u: 340 + 460 * u, decay=2.5)
    synth("Landing", 0.09, lambda t, u: 115 - 55 * u, brightness=0.05, noise=0.4, decay=5)
    synth("Attack", 0.14, lambda t, u: 800 - 560 * u + 70 * math.sin(90 * t), noise=0.65, decay=3)
    synth("Damage", 0.18, lambda t, u: 210 - 135 * u, brightness=0.45, noise=0.4, decay=3.2)
    synth("EnemyDeath", 0.22, lambda t, u: 560 * (1 - u) + 95, brightness=0.3, noise=0.5, decay=4)
    notes = [587.33, 739.99, 880.0, 1174.66]
    synth("Completion", 0.62, lambda t, u: notes[min(3, int(t / 0.11))], brightness=0.2, decay=1.8)
