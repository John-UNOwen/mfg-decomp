import struct, sys, collections, json

BASE = r"C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets\Resources\sound\xml"
OUT  = r"C:\Users\Pure Fox\AppData\Local\Temp\mfg-audio-audit"

class R:
    def __init__(self, b):
        self.b = b; self.i = 0
    def chunk(self):
        s = self.b[self.i:self.i+4].decode('ascii'); self.i += 4; return s
    def i32(self):
        v = struct.unpack_from('<i', self.b, self.i)[0]; self.i += 4; return v
    def f32(self):
        v = struct.unpack_from('<f', self.b, self.i)[0]; self.i += 4; return v
    def s(self):
        n = self.i32()
        if n == 0: return None
        s = self.b[self.i:self.i+n].decode('utf-8', 'replace'); self.i += n; return s

def parse_master(p):
    r = R(open(p,'rb').read())
    assert r.chunk()=='ver '; ver=r.i32(); r.i32(); name=r.s()
    assert r.chunk()=='mstr'; n=r.i32()
    out=[]
    for _ in range(n):
        out.append({'masterName': r.s(), 'volume': r.f32()})
    return ver,name,out

def parse_category(p):
    r = R(open(p,'rb').read())
    assert r.chunk()=='ver '; ver=r.i32(); r.i32(); name=r.s()
    assert r.chunk()=='ctgr'; n=r.i32()
    out=[]
    for _ in range(n):
        out.append({'categoryName': r.s(), 'volume': r.f32(), 'maxPlaybacksNum': r.i32(), 'masterName': r.s()})
    return ver,name,out

def parse_label(p):
    r = R(open(p,'rb').read())
    assert r.chunk()=='ver '; ver=r.i32(); r.i32(); name=r.s()
    assert r.chunk()=='lbl '; n=r.i32()
    out=[]
    for _ in range(n):
        d={'name': r.s()}
        d['clipName']=r.s()
        d['isLoop']=bool(r.i32())
        d['volume']=r.f32()
        d['maxPlaybacksBehavior']=r.i32()
        d['priority']=r.i32()
        d['categoryName']=r.s()
        d['singleGroup']=r.s()
        d['maxPlaybacksNum']=r.i32()
        d['isStealOldest']=bool(r.i32())
        d['unityMixerName']=r.s()
        d['spatialGroup']=r.s()
        d['playStartDelay']=r.f32()
        if ver>=3: d['playInterval']=r.f32()
        d['pan']=r.f32()
        d['pitchShiftCent']=r.i32()
        d['isPlayLastSamples']=bool(r.i32())
        d['fadeInTime']=r.f32()
        d['fadeOutTime']=r.f32()
        d['fadeInTimeOldSamples']=r.f32()
        d['fadeOutTimeOnPause']=r.f32()
        d['fadeInTimeOffPause']=r.f32()
        d['isVolumeRandom']=bool(r.i32()); d['inconsecutiveVolume']=bool(r.i32())
        d['volumeRandomMin']=r.f32(); d['volumeRandomMax']=r.f32(); d['volumeRandomUnit']=r.f32()
        d['isPitchRandom']=bool(r.i32()); d['inconsecutivePitch']=bool(r.i32())
        d['pitchRandomMin']=r.i32(); d['pitchRandomMax']=r.i32(); d['pitchRandomUnit']=r.i32()
        d['isPanRandom']=bool(r.i32()); d['inconsecutivePan']=bool(r.i32())
        d['panRandomMin']=r.f32(); d['panRandomMax']=r.f32(); d['panRandomUnit']=r.f32()
        d['isRandomPlay']=bool(r.i32()); d['inconsecutiveSource']=bool(r.i32())
        k=r.i32()
        d['randomSource']=[r.s() for _ in range(k)] if k else None
        d['isMovePitch']=bool(r.i32()); d['pitchStart']=r.i32(); d['pitchEnd']=r.i32(); d['pitchMoveTime']=r.f32()
        d['isMovePan']=bool(r.i32()); d['panStart']=r.f32(); d['panEnd']=r.f32(); d['panMoveTime']=r.f32()
        k=r.i32()
        d['duckingCategories']=[r.s() for _ in range(k)] if k else None
        d['duckingStartTime']=r.f32(); d['duckingEndTime']=r.f32(); d['duckingVolumeFactor']=r.f32()
        d['autoRestoreDucking']=bool(r.i32()); d['restoreTime']=r.f32()
        if ver>=2: d['isAndroidNative']=bool(r.i32())
        out.append(d)
    assert r.i == len(r.b), f"trailing bytes: consumed {r.i} of {len(r.b)}"
    return ver,name,out

vm, nm, masters = parse_master(BASE + r"\MasterSettings.bytes")
vc, nc, cats = parse_category(BASE + r"\CategorySettings.bytes")
vl, nl, labels = parse_label(BASE + r"\LabelSettings.bytes")

print(f"masters={len(masters)} categories={len(cats)} labels={len(labels)} labelVer={vl}")
print("masters:", json.dumps(masters, ensure_ascii=False))
print("categories:", json.dumps(cats, ensure_ascii=False))

cat_hist = collections.Counter(l['categoryName'] for l in labels)
print("category histogram:", dict(cat_hist))
pref = collections.Counter()
for l in labels:
    n = l['name'] or ''
    p = n.split('_')[0]
    pref[p] += 1
print("label name prefix histogram (top 30):", pref.most_common(30))
loop_by_cat = collections.Counter(l['categoryName'] for l in labels if l['isLoop'])
print("loop=true by category:", dict(loop_by_cat))
print("total loop=true:", sum(1 for l in labels if l['isLoop']))

# clipName coverage: how many label clipNames match an on-disk ogg?
import os
oggs = {}
root = r"C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project\Assets"
for dp, dn, fn in os.walk(root):
    for f in fn:
        if f.endswith('.ogg'):
            oggs[f[:-4]] = os.path.join(dp, f)
matched = sum(1 for l in labels if l['clipName'] in oggs)
clipnames = set(l['clipName'] for l in labels if l['clipName'])
print(f"labels with clipName matching an .ogg basename: {matched}/{len(labels)}; distinct clipNames: {len(clipnames)}")
missing_clips = sorted(c for c in clipnames if c not in oggs)
print("clipNames with no matching ogg (first 40):", missing_clips[:40])
print("count missing:", len(missing_clips))

with open(OUT + r"\labelsettings_dump.json", 'w', encoding='utf-8') as f:
    json.dump(labels, f, ensure_ascii=False, indent=1)

# BGM-ish labels sample: TITLE/MENU/HOME etc
for l in labels:
    n = l['name'] or ''
    if any(k in n for k in ('TITLE','MENU','HOME','LOGO','SPLASH','ATTRACT','THUMB')):
        print("BGMlabel|", n, "|clip:", l['clipName'], "|loop:", l['isLoop'], "|cat:", l['categoryName'], "|fadein:", l['fadeInTime'], "|fadeout:", l['fadeOutTime'], "|maxplay:", l['maxPlaybacksNum'], "|randplay:", l['isRandomPlay'], "|randomSource:", (l['randomSource'] or [])[:4])
