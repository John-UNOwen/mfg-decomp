import os, hashlib
import UnityPy
UP = r"C:\Users\Pure Fox\Documents\GitHub\mfg-decomp\build\unity-project"
AA = os.path.join(UP, "Library", "com.unity.addressables", "aa", "WebGL", "WebGL")
B = {
 "BGM_TITLE": "086d10c8915665ab5ba7564088d2369c_2eb9af12ec89270b83188d0b984aca8e.bundle_assets_all_2eb9af12ec89270b83188d0b984aca8e.bundle",
 "BGM_HOME":  "9bd7a556eefe98e51bc449b9bc97d87c_59cc049e75605080f4868d7db9cab644.bundle_assets_all_59cc049e75605080f4868d7db9cab644.bundle",
 "VOX_G01_APPEAL1": "2b8059feb8290e71b4e8b2408a0fae60_1da901269ab7b4dda20b38cdc4522038.bundle_assets_all_1da901269ab7b4dda20b38cdc4522038.bundle",
}
def find_ogg(name):
    for fam in (r"Assets\MFG\Sound\BGM", r"Assets\MFG\Characters"):
        d = os.path.join(UP, fam)
        for root, _, files in os.walk(d):
            if name + ".ogg" in files:
                return os.path.join(root, name + ".ogg")
def as_bytes(r):
    for attr in ("read", "get_data", "to_bytes"):
        f = getattr(r, attr, None)
        if callable(f):
            try: return f()
            except TypeError: pass
    buf = getattr(r, "m_Reader", None) or getattr(r, "reader", None)
    if buf is not None and isinstance(buf, memoryview):
        return buf.tobytes()
    mv = getattr(r, "memory_view", None)
    if mv is not None: return bytes(mv)
    raise RuntimeError("no byte accessor on %r" % type(r))
for name, bn in B.items():
    env = UnityPy.load(os.path.join(AA, bn))
    obj = next(o for o in env.objects if o.type.name == "AudioClip")
    o = obj.read()
    res = o.m_Resource
    cabname = res.m_Source.split("/")[-1]
    data = as_bytes(env.get_cab(cabname))
    seg = data[res.m_Offset:res.m_Offset+res.m_Size]
    ogg = open(find_ogg(name), "rb").read()
    same = hashlib.sha256(seg).hexdigest() == hashlib.sha256(ogg).hexdigest()
    print(name, "cab_size", len(data), "declared", res.m_Size, "shipped_ogg", len(ogg), "VERBATIM:", same)
    print("  seg head:", seg[:16])
    if not same:
        print("  OggS in cab:", data.count(b"OggS"), "cab_head:", data[:16])
