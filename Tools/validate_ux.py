#!/usr/bin/env python3
"""Source-only localization/audio checks; does not run Unity or establish visual usability."""
from pathlib import Path
import array
import json
import math
import re
import string
import sys
import wave

root = Path(__file__).resolve().parents[1]
resources = root / 'Assets/_Rumores/Resources'
table = json.loads((resources/'UiStrings.es-AR.json').read_text())
entries = table['entries']
keys = {item['key'] for item in entries}
assert len(keys) == len(entries), 'Duplicate translation keys'
for item in entries:
    assert item['value'].strip(), item['key']
    for _, field, _, _ in string.Formatter().parse(item['value']):
        assert field is None or field.isdigit(), item['key']
prefixes = ('app.', 'menu.', 'settings.', 'players.', 'avatar.', 'handoff.', 'progress.',
            'turn.', 'card.', 'history.', 'reveal.', 'results.', 'art.')
for path in (root/'Assets/_Rumores/Scripts/Presentation').glob('*.cs'):
    for token in re.findall(r'"([^"\n]+)"', path.read_text()):
        if token.startswith(prefixes) and not token.endswith('.'):
            assert token in keys, f'Missing UI string {token} in {path.name}'
for prefix, count in [('avatar.', 8), ('art.character.', 4), ('art.reaction.', 9)]:
    assert all(prefix+str(i) in keys for i in range(count))
print(f'PASS {len(keys)} UI strings, placeholders and source keys')

names = ('Music', 'Select', 'Transmit', 'Surprise', 'Panic', 'Reveal', 'Celebrate', 'Defeat')
for name in names:
    with wave.open(str(resources/'Audio'/(name+'.wav')), 'rb') as clip:
        assert (clip.getnchannels(), clip.getsampwidth(), clip.getframerate()) == (1, 2, 44100)
        values = array.array('h', clip.readframes(clip.getnframes()))
        if sys.byteorder != 'little':
            values.byteswap()
        duration = len(values)/44100
    peak = max(abs(x) for x in values)/32768
    rms = math.sqrt(sum((x/32768)**2 for x in values)/len(values))
    assert .01 < peak < .56 and .001 < rms < .20, (name, peak, rms)
    assert abs(sum(values)/len(values)/32768) < .001, f'DC offset: {name}'
    assert values[0] == values[-1] == 0, f'Abrupt clip edge: {name}'
    assert duration == 20 if name == 'Music' else .05 <= duration <= .8
    meta=(resources/'Audio'/(name+'.wav.meta')).read_text()
    assert 'compressionFormat: 0' in meta and 'preloadAudioData: 1' in meta
    print(f'PASS {name}: {duration:.2f}s, peak {peak:.3f}, RMS {rms:.3f}, silent edges')
print('Audio is synthesized provisional material; listening and Unity playback remain separate checks.')
