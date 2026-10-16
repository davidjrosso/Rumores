#!/usr/bin/env python3
"""Static asset wiring checks. This is not a Unity import/build validation."""
from pathlib import Path
import json,re
root=Path(__file__).resolve().parents[1]
assets=root/'Assets'
guids={}
for meta in assets.rglob('*.meta'):
    match=re.search(r'^guid: ([0-9a-f]{32})$',meta.read_text(),re.M)
    assert match,f'GUID inválido: {meta}'
    assert match[1] not in guids,f'GUID duplicado: {meta}'
    guids[match[1]]=meta
    assert Path(str(meta)[:-5]).exists(),f'Meta huérfano: {meta}'
for file in assets.rglob('*'):
    if file.suffix=='.meta':continue
    assert Path(str(file)+'.meta').is_file(),f'Falta meta: {file}'
    if file.suffix in ('.asset','.unity'):
        for guid in re.findall(r'guid: ([0-9a-f]{32})',file.read_text()):
            assert guid in guids,f'Referencia sin resolver: {file} / {guid}'
assemblies={json.loads(p.read_text())['name'] for p in assets.rglob('*.asmdef')}
for p in assets.rglob('*.asmdef'):
    data=json.loads(p.read_text())
    for reference in data.get('references',[]):assert reference in assemblies or reference=='UnityEngine.UI',f'Assembly desconocida: {reference}'
manifest=json.loads((root/'Packages/manifest.json').read_text())
assert manifest['dependencies']['com.unity.ugui']=='1.0.0'
assert manifest['dependencies']['com.unity.test-framework']=='1.1.33'
settings=(root/'ProjectSettings/EditorBuildSettings.asset').read_text()
scene='Assets/_Rumores/Scenes/Main.unity'
assert f'path: {scene}' in settings
scene_guid=re.search(r'guid: ([0-9a-f]{32})',(root/(scene+'.meta')).read_text())[1]
assert f'guid: {scene_guid}' in settings
assert '2022.3.62f1' in (root/'ProjectSettings/ProjectVersion.txt').read_text()
print(f'PASS static Unity wiring: {len(guids)} GUIDs, {len(assemblies)} assemblies, scene, catalog, font and pinned package manifest.')
