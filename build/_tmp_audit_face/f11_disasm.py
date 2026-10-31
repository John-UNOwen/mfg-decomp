import json, struct, os, re, ctypes
from ctypes import *
OUT = r"C:\mfg\build\_tmp_audit_face"
g = open(os.path.join(OUT,"face_blob.bin"),"rb").read()
progs = json.load(open(os.path.join(OUT,"progs2.json")))
dc = windll.LoadLibrary("d3dcompiler_47")

class ID3DBlob(Structure): pass
D3DDisassemble = dc.D3DDisassemble
D3DDisassemble.restype = c_long
D3DDisassemble.argtypes = [c_void_p, c_size_t, c_uint, c_char_p, POINTER(c_void_p)]
def blob_text(ptr):
    q = struct.unpack_from('<4Q', string_at(ptr, 32))
    return string_at(q[3], q[2])

def sgn_entries(off, size):
    # fourcc+size consumed; body: entryCount? Unity H3 style: [u32 n][u32 ???]
    body = g[off+8:off+size]
    n, psize = struct.unpack_from("<2I", body, 0)
    esz = psize // n if n else 0
    names = body[8+psize:]
    out=[]
    for i in range(n):
        e = body[8+i*esz:8+(i+1)*esz]
        nameOff, index = struct.unpack_from("<2I", e, 0)
        nm = names[nameOff:names.index(b"\x00", nameOff)].decode("latin-1")
        rest = struct.unpack_from("<"+"I"*(esz//4-2), e, 8)
        out.append((nm, index)+rest)
    return out

# OSGN/ISGN format decode: fields after nameOff,index for PS inputs: registerIndex? kind, type(mask?)...
stage_map = {}# reload raw section info
ents=[]; o=4
while o <= 4+83*12:
    off,size,flg = struct.unpack_from("<3I", g, o)
    if flg!=0: break
    ents.append((off,size)); o+=12
rec={}
for i,(off,size) in enumerate(ents):
    dx = g.find(b"DXBC", off, off+min(size,200))
    if dx<0: continue
    ln, nsec = struct.unpack_from("<2I", g, dx+24)
    secs={}
    for si in range(nsec):
        so, = struct.unpack_from("<I", g, dx+32+si*4)
        tag = g[dx+so:dx+so+4].decode("latin-1","ignore")
        ssize, = struct.unpack_from("<I", g, dx+so+4)
        secs[tag]=(dx+so, ssize)
    rec[i]=(dx,ln,secs)

summary=[]
for i,(dx,ln,secs) in sorted(rec.items()):
    data = g[dx:dx+ln]
    blob = c_void_p()
    hr = D3DDisassemble(cast(c_char_p(data), c_void_p), len(data), 0, b"", byref(blob))
    txt = None
    if hr == 0 and blob.value:
        txt = blob_text(blob.value).decode("latin-1")
    head = txt.splitlines()[:3] if txt else None
    hdrline = ""
    if txt:
        m = re.search(r"^// (vs|ps|hs|ds|gs|cs)_(\d)_?_?(\d)?", txt, re.M)
        if m: hdrline = m.group(0)
    ins, outs = [], []
    if txt:
        mode = None
        for ln in txt.splitlines():
            if ln.startswith("// Name"): mode = "in"; continue
            if ln.startswith("// ---"): continue
            if mode == "in":
                nm = ln.split()[0] if ln.strip() and not ln.startswith("//") else None
                if nm is None or nm == "Output": mode = None; continue
                if ln.lstrip().startswith("//"): mode = None; continue
                ins.append(ln.split()[0] + ("_" + ln.split()[1] if ln.split()[1].isdigit() else ""))
    summary.append((i, hdrline, len(txt) if txt else hr, ins, outs))
    if txt:
        open(os.path.join(OUT, f"asm{i:02d}.txt"), "w", encoding="utf-8").write(txt)
    else:
        print(f"P{i:02d} DISASM FAIL hr={hex(hr & 0xffffffff)}")
for i, hdr, ln, ins, outs in summary:
    print(f"P{i:02d} {hdr:20s} asm={ln}")
    print(f"   IN : {ins}")
    print(f"   OUT: {outs}")
