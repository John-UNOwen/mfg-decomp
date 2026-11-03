import re, json, sys
p = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Prefabs\CharaModel.prefab"
text = open(p, encoding='utf-8', errors='replace').read()
lines = text.splitlines()
# split objects
hdr = re.compile(r'^--- !u!(\d+) &(\d+)( stripped)?')
objs = []  # (class, fileid, start_line, end_line, body)
cur = None
for idx, ln in enumerate(lines, 1):
    m = hdr.match(ln)
    if m:
        if cur: cur[3] = idx-1; objs.append(cur)
        cur = [int(m.group(1)), m.group(2), idx, None, []]
    elif cur:
        cur[4].append(ln)
if cur: cur[3] = len(lines); objs.append(cur)
print("num objects", len(objs))
byid = {o[1]: o for o in objs}
CLASS = {1:'GameObject',4:'Transform',23:'MeshRenderer',33:'MeshFilter',95:'Animator',137:'SkinnedMeshRenderer',114:'Collider',64:'MeshCollider',114:'Collider'}
def get(body, key):
    for ln in body:
        s = ln.strip()
        if s.startswith(key+':'):
            return s.split(':',1)[1].strip()
    return None
def getref(body, key):
    for ln in body:
        s = ln.strip()
        if s.startswith(key+':'):
            parts = s.split('{')
            m = re.search(r'fileID:(-?\d+)', s)
            return m.group(1) if m else None
    return None
def get_children(tfid):
    o = byid.get(tfid)
    if not o: return []
    out=[]; inchildren=False
    for ln in o[4]:
        s=ln.strip()
        if s=='m_Children:': inchildren=True; continue
        if inchildren:
            if s.startswith('- fileID:'):
                m=re.search(r'fileID:(-?\d+)',s)
                if m.group(1)!='0': out.append(m.group(1))
            elif not s.startswith('-'): inchildren=False
    return out
def go_of(o):  # for component obj -> gameObject fileID
    return getref(o[4],'m_GameObject')
def transform_of(goid):
    o = byid.get(goid)
    for cid in ['m_Component']:
        pass
    # find components list
    comps = []
    incomp=False
    for ln in o[4]:
        s=ln.strip()
        if s=='m_Component:': incomp=True; continue
        if incomp:
            if s.startswith('- component:'): continue
            m=re.search(r'fileID:(-?\d+)',s)
            if m and s.startswith('- '): comps.append(m.group(1))
            elif not s.startswith('-'): incomp=False
    return comps
# report: list all GameObject names containing outlien/outline/group
print("=== GameObjects with interesting names ===")
for o in objs:
    if o[0]==1:
        nm = get(o[4],'m_Name')
        if nm and ('outl' in nm.lower() or 'group' in nm.lower() or nm in ('body','hair','face_group','Chara','CharaModel')):
            print(f"line {o[2]} GO &{o[1]} name={nm!r} active={get(o[4],'m_IsActive')}")
print("=== SkinnedMeshRenderers with line ranges ===")
targets = [15071,15401,15708,16015,16322,16629]
for o in objs:
    if o[0]==137:
        print(f"SMR &{o[1]} lines {o[2]}-{o[3]}")
