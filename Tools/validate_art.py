#!/usr/bin/env python3
"""Validate source PNGs and serialized import settings, not Unity rendering."""
from pathlib import Path
import struct
import zlib

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/_Rumores/Resources/Art/Preview'


def pixels(path):
    data = path.read_bytes()
    assert data[:8] == b'\x89PNG\r\n\x1a\n', path
    offset, compressed = 8, bytearray()
    while offset < len(data):
        length, = struct.unpack_from('>I', data, offset)
        kind = data[offset+4:offset+8]
        payload = data[offset+8:offset+8+length]
        crc, = struct.unpack_from('>I', data, offset+8+length)
        assert zlib.crc32(kind+payload) & 0xffffffff == crc, f'PNG CRC: {path}'
        if kind == b'IHDR':
            width, height, depth, color, compression, filtering, interlace = struct.unpack('>IIBBBBB', payload)
            assert depth == 8 and color in (2, 6) and compression == filtering == interlace == 0
        if kind == b'IDAT':
            compressed.extend(payload)
        offset += length+12
        if kind == b'IEND':
            break
    raw = zlib.decompress(compressed)
    channels = 4 if color == 6 else 3
    stride = width*channels
    assert len(raw) == height*(stride+1)
    previous = bytearray(stride)
    rows = []
    for y in range(height):
        start = y*(stride+1)
        method = raw[start]
        assert 0 <= method <= 4
        row = bytearray(raw[start+1:start+1+stride])
        for x in range(stride):
            left = row[x-channels] if x >= channels else 0
            above = previous[x]
            corner = previous[x-channels] if x >= channels else 0
            if method == 4:
                p = left+above-corner
                distances = [abs(p-left), abs(p-above), abs(p-corner)]
                prediction = (left, above, corner)[distances.index(min(distances))]
            else:
                prediction = (0, left, above, (left+above)//2)[method]
            row[x] = (row[x]+prediction) & 255
        rows.append(row)
        previous = row
    return width, height, channels, rows


characters = [(name, (1024, 1536, 4), 0) for name in ('Miedoso', 'Exagerado', 'Mentiroso', 'Ignorante')]
for name, expected, pivot in characters + [('Pueblo', (1536, 1024, 3), 0.5)]:
    path = ART / (name+'.png')
    width, height, channels, rows = pixels(path)
    assert (width, height, channels) == expected, name
    if channels == 4:
        alpha = [a for row in rows for a in row[3::4]]
        clear = alpha.count(0)/len(alpha)
        assert clear > .4 and max(alpha) >= 250, 'Falta transparencia o cuerpo visible'
        assert all(rows[y][x*4+3] == 0 for x, y in [(0, 0), (width-1, 0), (0, height-1), (width-1, height-1)])
        print(f'PASS {name}: {width}x{height}, RGBA, {clear:.2%} transparent pixels, clear corners')
    else:
        print(f'PASS {name}: {width}x{height}, opaque RGB')
    meta = Path(str(path)+'.meta').read_text()
    for setting in ('textureType: 8', 'spriteMode: 1', 'spritePixelsToUnits: 100',
                    'enableMipMap: 0', 'sRGBTexture: 1', 'filterMode: 1',
                    'wrapU: 1', 'wrapV: 1', 'textureCompression: 0', 'maxTextureSize: 2048',
                    f'spritePivot: {{x: 0.5, y: {pivot}}}'):
        assert setting in meta, f'{name}: falta {setting}'
print('PASS source images/import metadata. Unity import, UI rendering and animation are NOT checked here.')
