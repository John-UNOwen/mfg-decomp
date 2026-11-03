import re, json
p = r"C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\C001\Prefabs\CharaModel.prefab"
text = open(p, encoding='utf-8', errors='replace').read()
lines = text.splitlines()
hdr = re.compile(r'^--- !u!(\d+) &(\d+)')
objs = []
cur = None
for idx, ln in enumerate(lines, 1):
    m = hdr.match(ln)
    if m:
        if cur: cur[3]=idx-1; objs.append(cur)
        cur=[int(m.group(1)), m.group(2), idx, None, []]
    elif cur: cur[4].append(ln)
if cur: cur[3]=len(lines); objs.append(cur)
byid={o[1]:o for o in objs}
CLS={1:'GameObject',4:'Transform',224:'RectTransform',23:'MeshRenderer',33:'MeshFilter',137:'SkinnedMeshRenderer',95:'Animator',114:'Collider',64:'MeshCollider',290:'SkinnedCloth',181:'BillboardAsset'}
def refs(body, key):
    out=[]; inn=False
    for ln in body:
        s=ln.strip()
        if s.startswith(key+':'): 
            if '{' in s:
                m=re.search(r'fileID:\s*(-?\d+)',s)
                if m and m.group(1)!='0': out.append(m.group(1))
            else: inn=True; continue
        elif inn:
            if s.startswith('- '):
                m=re.search(r'fileID:\s*(-?\d+)',s)
                if m and m.group(1)!='0': out.append(m.group(1))
            elif s and not s.startswith('-'): inn=False
    return out
def scalar(body, key):
    for ln in body:
        s=ln.strip()
        if s.startswith(key+':'): return s.split(':',1)[1].strip()
def comps_of(goid):
    o=byid[goid]
    b=o[4]; out=[]; inn=False
    for ln in b:
        s=ln.strip()
        if s=='m_Component:': inn=True; continue
        if inn:
            if s.startswith('- component:'):
                m=re.search(r'fileID:\s*(-?\d+)',s)
                if m: out.append(m.group(1))
            elif not s.startswith(' ') and not s.startswith('-'): inn=False
    return out
def children(tfid): return refs(byid[tfid][4],'m_Children')
def parent(tfid): 
    r=refs(byid[tfid][4],'m_Father')
    return r[0] if r else None
def find_transform(goid):
    for c in comps_of(goid):
        if byid[c][0] in (4,224): return c
def name_of(goid): return scalar(byid[goid][4],'m_Name')
def active_of(goid): return scalar(byid[goid][4],'m_IsActive')
GUIDMAP = {
 '1c28d86962ca97e47a13d37336c756a7':'Mt_hair','8b5bef34bcf91f04f894b83d4b5119aa':'Mt_ribbon',
 '1c6cd6eb1b5eeb74385dc2498cbcaecc':'Mt_outline_face','22ece558a18fbdd40a7ba76b79fc672a':'Mt_outline_hair'}
def mat_names(o):
    out=[]
    b=o[4]; inn=False
    for ln in b:
        s=ln.strip()
        if s=='m_Materials:': inn=True; continue
        if inn:
            m=re.search(r'guid: ([0-9a-f]+)',s)
            if m:
                g=m.group(1); out.append(GUIDMAP.get(g,g))
            elif not s.startswith(' '): inn=False
    return out
root_tf = find_transform('1246887588868771')  # group outlien
print("group outlien transform:", root_tf, "parent:", parent(root_tf), "=", name_of(byid[parent(root_tf)][1]) if parent(root_tf) else None)
def walk(tf, depth=0):
    goid = byid[tf][1]
    # find GO for this transform: transform's m_GameObject
    go = refs(byid[tf][4],'m_GameObject')[0]
    nm = name_of(go); act = active_of(go)
    info=[]
    for c in comps_of(go):
        cls=CLS.get(byid[c][0],str(byid[c][0]))
        if byid[c][0] in (23,137):
            en = scalar(byid[c][4],'m_Enabled')
            info.append(f"{cls}@line{byid[c][2]} enabled={en} mats={mat_names(byid[c])}")
    print("  "*depth + f"[{nm}] active={act} GO&{go} tf&{tf} | " + " ; ".join(info))
    for ch in children(tf): walk(ch, depth+1)
walk(root_tf)
print()
print("=== Normal group tree (names + renderers only, compressed) ===")
g2 = find_transform('1724804207875695')
def walk2(tf, depth=0):
    go = refs(byid[tf][4],'m_GameObject')[0]
    nm=name_of(go); act=active_of(go)
    info=[]
    for c in comps_of(go):
        if byid[c][0] in (23,137):
            info.append(f"{CLS.get(byid[c][0])}@L{byid[c][2]} enabled={scalar(byid[c][4],'m_Enabled')} mats={mat_names(byid[c])}")
    if info or depth<=2:
        print("  "*depth+f"[{nm}] active={act} | "+" ; ".join(info))
    for ch in children(tf): walk2(ch, depth+1)
walk2(g2)
