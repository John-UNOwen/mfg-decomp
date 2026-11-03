import json, re, sys

def probe(path, tag):
    with open(path, "r", encoding="utf-8-sig") as f:
        cat = json.load(f)
    kds = cat.get("m_KeyDataString")
    blob = bytes.fromhex(kds) if isinstance(kds, str) else b""
    print(tag, "blob bytes:", len(blob))
    hits = [m.start() for m in re.finditer(b"Tile", blob)]
    print(tag, "Tile hits:", len(hits))
    shown = set()
    for h in hits[:4000]:
        s = blob[max(0, h-40):h+80]
        s = re.sub(rb"[^ -~]", b".", s).decode()
        if s not in shown:
            shown.add(s)
            if len(shown) <= 25:
                print("   ", s)
    # count distinct keys containing TileSurface / 2D sprites
    keys = set(re.findall(rb"[A-Za-z0-9_./\\\\:%-]*Tile[A-Za-z0-9_./\\\\:%-]*", blob))
    print(tag, "distinct Tile* strings:", len(keys))
    for k in sorted(keys)[:60]:
        print("   KEY", k.decode())

probe(r"C:\mfg\build\unity-project\out\webgl\StreamingAssets\aa\catalog.json", "WEBGL")
probe(r"C:\mfg\build\unity-project\out\standalone\MFG_Data\StreamingAssets\aa\catalog.json", "DESKTOP")
