import json

def keys_of(path):
    with open(path, "r", encoding="utf-8-sig") as f:
        cat = json.load(f)
    ks = []
    for k in cat.get("m_DefaultLocalGroup", {}).get("m_Items", []):
        pass
    # simple approach: keys live under "m_KeyDataString" (binary hex) in Addressables 1.x.
    return cat

def main():
    for label, path in [
        ("desktop", r"C:\mfg\build\unity-project\out\standalone\MFG_Data\StreamingAssets\aa\catalog.json"),
        ("webgl", r"C:\mfg\build\unity-project\out\webgl\StreamingAssets\aa\catalogjson" if False else r"C:\mfg\build\unity-project\out\webgl\StreamingAssets\aa\catalog.json"),
    ]:
        cat = keys_of(path)
        print(label, "top-level keys:", sorted(cat.keys()))
        # keys in Addressables TextAssetData: m_KeyDataString is hex of serialized strings; easier: raw text scan
        raw = open(path, "r", encoding="utf-8-sig").read()
        import re
        hits = sorted(set(re.findall(r"[A-Za-z0-9_./\\\\]*TileSurface[A-Za-z0-9_./\\\\]*", raw)))
        print(label, "TileSurface-ish substrings:", len(hits))
        for h in hits[:30]:
            print("   ", h)
        # also scan decoded key blob: m_KeyDataString hex
        kds = cat.get("m_KeyDataString")
        if kds:
            blob = bytes.fromhex(kds)
            strs = sorted(set(s for s in re.findall(rb"[ -~]{12,}", blob) if b"/" in s or b"png" in s or b"CustomizeItem" in s))
            tile = [s.decode() for s in strs if b"TileSurface" in s]
            print(label, "decoded keys containing TileSurface:", len(tile))
            for t in tile[:40]:
                print("   KEY", t)
            att = [s.decode() for s in strs if "Assets/MFG/CustomizeItem" in s.decode(errors="ignore")][:15]
            print(label, "sample CustomizeItem keys:", len([s for s in strs if b"CustomizeItem" in s]))

if __name__ == "__main__":
    main()
