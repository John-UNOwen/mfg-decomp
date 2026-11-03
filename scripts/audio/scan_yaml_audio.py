import os, re, json, collections

A = r"C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets"
OUT = r"C:\Users\Pure Fox\AppData\Local\Temp\mfg-audio-audit"

# 1) guid -> clip name map from .ogg.meta
guid2name = {}
for dp, dn, fn in os.walk(A):
    for f in fn:
        if f.endswith('.ogg.meta'):
            p = os.path.join(dp, f)
            t = open(p, encoding='utf-8', errors='replace').read()
            m = re.search(r'guid: ([0-9a-f]{32})', t)
            if m:
                guid2name[m.group(1)] = f[:-9]  # strip .ogg.meta
print('ogg meta guids:', len(guid2name))

guid_re = re.compile(r'[0-9a-f]{32}')
scenes = []
for dp, dn, fn in os.walk(A):
    for f in fn:
        if f.endswith(('.unity', '.prefab')):
            scenes.append(os.path.join(dp, f))
print('yaml files scanned:', len(scenes))

audio_ref = {}
u82 = {}
listener = {}
for p in scenes:
    t = open(p, encoding='utf-8', errors='replace').read()
    n82 = t.count('--- !u!82 ')
    if n82:
        u82[p] = n82
    if 'AudioListener' in t:
        listener[p] = t.count('--- !u!81 ')
    guids = set(guid_re.findall(t)) & set(guid2name)
    if guids:
        rel = os.path.relpath(p, A)
        audio_ref[rel] = sorted(guid2name[g] for g in guids)

print('files with --- !u!82 (AudioSource):', len(u82))
print('files referencing an AudioClip guid:', len(audio_ref))
for k, v in sorted(audio_ref.items()):
    print(f"REF {k} ({len(v)} clips): {', '.join(v[:12])}{' ...' if len(v)>12 else ''}")

with open(os.path.join(OUT, 'audio_guid_refs.json'), 'w', encoding='utf-8') as f:
    json.dump(audio_ref, f, ensure_ascii=False, indent=1)

# scenes inventory
print('--- scene list ---')
for dp, dn, fn in os.walk(os.path.join(A, 'MFG', 'Scenes')):
    for f in fn:
        if f.endswith('.unity'):
            print(os.path.relpath(os.path.join(dp, f), A))
