#!/usr/bin/env python3
"""Original synthesized cues, no recordings/samples or AI. Python standard library only."""
from pathlib import Path
import math
import struct
import wave

ROOT = Path(__file__).resolve().parents[1] / 'Assets/_Rumores/Resources/Audio'
RATE = 44100


def render(name, duration, notes):
    samples = [0.0] * round(duration * RATE)
    for start, length, midi, gain in notes:
        frequency = 440 * 2 ** ((midi - 69) / 12)
        for i in range(round(length * RATE)):
            index = round(start * RATE) + i
            if index >= len(samples):
                break
            t = i / RATE
            envelope = min(1, t / .012) * min(1, (length - t) / .04) * math.exp(-4 * t / length)
            tone = math.sin(2 * math.pi * frequency * t) + .17 * math.sin(4 * math.pi * frequency * t)
            samples[index] += tone * envelope * gain
    # Peak ceiling leaves headroom for mixing and ensures there is no clipping.
    peak = max(abs(x) for x in samples)
    scale = min(1, .55 / peak) if peak else 1
    pcm = b''.join(struct.pack('<h', round(x * scale * 32767)) for x in samples)
    with wave.open(str(ROOT / (name + '.wav')), 'wb') as output:
        output.setparams((1, 2, RATE, len(samples), 'NONE', 'not compressed'))
        output.writeframes(pcm)


ROOT.mkdir(parents=True, exist_ok=True)
# Eight bars at 96 BPM: gentle plucked chords and a newly composed sparse melody.
notes = []
chords = [(48, 55, 60), (53, 57, 60), (50, 57, 62), (55, 59, 62)] * 2
melody = [72, 76, 74, 72, 69, 72, 74, 67, 72, 79, 76, 74, 69, 74, 71, 72]
for bar, chord in enumerate(chords):
    for beat in range(4):
        notes.append((bar * 2.5 + beat * .625, .55, chord[beat % 3], .12))
    for beat in range(2):
        notes.append((bar * 2.5 + .3125 + beat * 1.25, .65, melody[bar * 2 + beat], .08))
render('Music', 20, notes)
render('Select', .10, [(0, .08, 72, .18)])
render('Transmit', .36, [(0, .14, 60, .22), (.10, .14, 65, .19), (.20, .14, 69, .15)])
render('Surprise', .34, [(0, .14, 67, .22), (.12, .20, 79, .22)])
render('Panic', .44, [(0, .16, 65, .22), (.13, .16, 66, .16), (.27, .15, 65, .12)])
render('Reveal', .64, [(0, .25, 60, .18), (.12, .25, 64, .18), (.24, .36, 67, .20)])
render('Celebrate', .72, [(0, .20, 72, .20), (.16, .20, 76, .20), (.32, .36, 79, .20)])
render('Defeat', .64, [(0, .22, 64, .16), (.18, .22, 62, .16), (.36, .24, 60, .14)])
print('Generated 8 original mono PCM16 / 44.1 kHz WAVs.')
