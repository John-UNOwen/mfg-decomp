# List (and with --apply, enable) the SkinnedMeshRenderers under 'group outlien' in a prefab.
import re, sys
path = sys.argv[1]; apply = '--apply' in sys.argv
t = open(path, encoding='utf8', newline='').read()
parts = re.split(r'(\n--- !u!\d+ &\d+[^\n]*)', t)
hdr = {}
docs = []  # (header, body)
docs.append(('', parts[0]))
for i in range(1, len(parts), 2): docs.append((parts[i], parts[i + 1]))
byid = {}
for k, (h, b) in enumerate(docs):
    m = re.match(r'\n--- !u!(\d+) &(\d+)', h)
    if m: byid[m.group(2)] = (m.group(1), k)
def body(fid): return docs[byid[fid][1]][1]
def name_of_go(go): return re.search(r'm_Name: (.*)', body(go)).group(1).strip()
def tf_of_go(go):
    for fid in re.findall(r'component: \{fileID: (\d+)\}', body(go)):
        if byid.get(fid, ('',))[0] == '4': return fid
def path_of_go(go):
    names = []; tf = tf_of_go(go)
    while tf and tf != '0':
        b = body(tf); g = re.search(r'm_GameObject: \{fileID: (\d+)\}', b).group(1)
        names.append(name_of_go(g)); tf = re.search(r'm_Father: \{fileID: (\d+)\}', b).group(1)
    return '/'.join(reversed(names))
changed = 0
for fid, (ty, k) in byid.items():
    if ty != '137': continue
    h, b = docs[k]
    go = re.search(r'm_GameObject: \{fileID: (\d+)\}', b).group(1)
    en = re.search(r'\n  m_Enabled: (\d)', b).group(1)
    act = re.search(r'm_IsActive: (\d)', body(go)).group(1)
    mats = re.findall(r'guid: (\w+), type: 2\}', b.split('m_Materials:')[1].split('m_StaticBatchInfo')[0])
    p = path_of_go(go)
    print(f'enabled={en} goActive={act} {p}  mats={[m[:8] for m in mats]}')
    if apply and 'group outlien' in p and en == '0':
        docs[k] = (h, b.replace('\n  m_Enabled: 0', '\n  m_Enabled: 1', 1)); changed += 1
if apply:
    open(path, 'w', encoding='utf8', newline='').write(''.join(h + b for h, b in docs))
    print('enabled', changed, 'renderers')
