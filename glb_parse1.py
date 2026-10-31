import json, struct
p = r"C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Prefabs\CharaModel.glb"
b = open(p,'rb').read()
print("filesize", len(b))
off = 12
while off + 8 <= len(b):
    clen = struct.unpack('<I', b[off:off+4])[0]
    ctype = b[off+4:off+8]
    print("chunk", ctype, "dataoff", off+8, "len", clen)
    if ctype == b'JSON':
        g = json.loads(b[off+8:off+8+clen])
    off += 8 + clen
open(r"C:\mfg\glb_parsed.json","w").write(json.dumps(g, indent=1))
print("keys", sorted(g.keys()))
print("nodes", len(g.get("nodes",[])), "meshes", len(g.get("meshes",[])), "materials", len(g.get("materials",[])))
print("scene", json.dumps(g.get("scene")), "scenes", json.dumps(g.get("scenes")))
for i,n in enumerate(g.get("nodes",[])):
    nm = n.get("name","")
    if nm and ("out" in nm.lower() or "line" in nm.lower() or "silho" in nm.lower()):
        print("NODE", i, repr(nm), json.dumps(n))
