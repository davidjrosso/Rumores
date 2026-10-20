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
    if file.suffix in ('.asset','.unity','.prefab'):
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
def guid_for(relative):
    return re.search(r'guid: ([0-9a-f]{32})',(root/(relative+'.meta')).read_text())[1]
player_settings=(root/'ProjectSettings/ProjectSettings.asset').read_text()
for guid in re.findall(r'guid: ([0-9a-f]{32})',player_settings):
    assert guid in guids,f'PlayerSettings referencia un recurso inexistente: {guid}'
assert guid_for('Assets/_Rumores/Art/AppIcon.png') in player_settings
for setting in ('productName: Rumores','Android: com.davidjrosso.rumores',
                'AndroidTargetArchitectures: 2','AndroidMinSdkVersion: 23',
                'forceInternetPermission: 0','defaultScreenOrientation: 1'):
    assert setting in player_settings,f'Configuración base Android: falta {setting}'
main=(root/scene).read_text()
for relative in ('Assets/_Rumores/Scripts/Presentation/PartyApp.cs','Assets/_Rumores/Resources/PartyCatalog.asset','Assets/_Rumores/Prefabs/PartyActionCard.prefab'):
    assert guid_for(relative) in main,f'Main no referencia {relative}'
party_catalog=(assets/'_Rumores/Resources/PartyCatalog.asset').read_text()
for relative in ('Assets/_Rumores/Scripts/Presentation/PartyCatalogAsset.cs','Assets/_Rumores/Resources/PartyContent.json'):
    assert guid_for(relative) in party_catalog,f'PartyCatalog no referencia {relative}'
prefab=(assets/'_Rumores/Prefabs/PartyActionCard.prefab').read_text()
assert guid_for('Assets/_Rumores/Scripts/Presentation/PartyActionCard.cs') in prefab
assert 'RectTransform:' in prefab
assert guid_for('Assets/_Rumores/Scripts/Presentation/RumoresApp.cs') in (assets/'_Rumores/Scenes/Legacy.unity').read_text()
assert guid_for('Assets/_Rumores/Scripts/Presentation/RumoresApp.cs') not in main
art_path='Assets/_Rumores/Resources/RumoresArt.asset'
assert guid_for(art_path) in main
art=(root/art_path).read_text()
for relative in ('Assets/_Rumores/Scripts/Presentation/RumoresArt.cs','Assets/_Rumores/Resources/Art/Preview/Miedoso.png','Assets/_Rumores/Resources/Art/Preview/Pueblo.png'):
    assert guid_for(relative) in art,f'Art library missing {relative}'
preview=(assets/'_Rumores/Scenes/ArtPreview.unity').read_text()
assert guid_for(art_path) in preview
assert guid_for('Assets/_Rumores/Scripts/Presentation/ArtPreview.cs') in preview
for name in ('Exagerado','Mentiroso','Ignorante'):
    assert guid_for('Assets/_Rumores/Resources/Art/Preview/'+name+'.png') in art
print(f'PASS static Unity wiring: {len(guids)} GUIDs, {len(assemblies)} assemblies, Main/Legacy, party catalog, action prefab, font and pinned package manifest.')
