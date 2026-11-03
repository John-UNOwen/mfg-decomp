import os, re, json, collections

A = r"C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets"
OUT = r"C:\Users\Pure Fox\AppData\Local\Temp\mfg-audio-audit"

labels = json.load(open(os.path.join(OUT,'labelsettings_dump.json'), encoding='utf-8'))
label_names = {l['name'] for l in labels if l['name']}
clip_missing = {l['clipName'] for l in labels if l['clipName'] and not os.path.exists(
    os.path.join(A, 'MFG', 'Sound', 'BGM', l['clipName'] + '.ogg'))}
# cheaper: full ogg set
oggs = set()
for dp, dn, fn in os.walk(A):
    for f in fn:
        if f.endswith('.ogg'):
            oggs.add(f[:-4])

lit_re = re.compile(r'"((?:SFX|VOX|BGM)_[A-Za-z0-9_%{}()\[\]+. ]*?)"')
refs = collections.Counter()
files_with = collections.defaultdict(set)
for dp, dn, fn in os.walk(os.path.join(A, 'Assembly-CSharp')):
    for f in fn:
        if not f.endswith('.cs'): continue
        t = open(os.path.join(dp, f), encoding='utf-8', errors='replace').read()
        for m in lit_re.finditer(t):
            s = m.group(1)
            if len(s) > 3:
                refs[s] += 1
                files_with[s].add(f)

# expand GXX templates
expanded = {}
for s, n in refs.items():
    if 'GXX' in s:
        for i in range(1, 22):
            expanded[s.replace('GXX', f'G{i:02d}')] = n
    else:
        expanded[s] = n

missing_label = sorted(s for s in expanded if s not in label_names and not s.startswith(('BGM_THUMB','VOX_G')))
missing_exact = sorted(s for s in expanded if s not in label_names)
missing_clip = sorted(s for s in expanded if s in label_names and any(l['name']==s and l['clipName'] not in oggs for l in labels))

print('distinct audio label literals in code:', len(refs), '; expanded GXX:', len(expanded))
print('code refs NOT in LabelSettings table:', len(missing_exact))
for s in missing_exact[:60]:
    print('  NO-TABLE', s, 'x', refs.get(s, expanded.get(s)), 'in', sorted(files_with.get(s if s in files_with else next((k for k in files_with if s in k), ''), []))[:3])
print('code refs IN table but clip missing from tree:', len(missing_clip))
for s in missing_clip:
    print('  NO-CLIP ', s)
# also count references to labels whose clip is missing (40 known)
print('table labels with missing clips (recap):', sorted({l['name'] for l in labels if l['clipName'] and l['clipName'] not in oggs}))
