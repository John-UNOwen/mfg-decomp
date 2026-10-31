import os, re
ROOT = r"C:\mfg\build\unity-project\Assets"
targets = {
 "eeb96954b951f114896f527593eb2614": "Mt_face shader",
 "b8b7e4c7ec0b8ca459cda8277ebe3ed8": "Mouth.shader",
 "4f7cb33aed21bad45882eb4c2a8dbafd": "eyeL mat (instance A)",
 "e8d6d0f4aed78d54eb9c84f0a80b77c7": "eyeR mat (instance A)",
 "1c6cd6eb1b5eeb74385dc2498cbcaecc": "shared eye mat (instance B)",
 "25476fc3aa7289245a68306632e21194": "mesh eye_L guid",
 "eaefbe881c18e754ea30d11fad9c757d": "mesh eye_R guid",
}
pat = re.compile(r"guid: ([0-9a-f]{32})", re.I)
for dirpath, dirs, files in os.walk(ROOT):
    for fn in files:
        if fn.endswith((".meta", ".mat", ".prefab")) and not fn.endswith(".meta"):
            continue
        p = os.path.join(dirpath, fn)
        if not fn.endswith(".meta"):
            continue
        try:
            head = open(p, "r", encoding="utf-8", errors="ignore").read(400)
        except Exception:
            continue
        m = re.search(r"^guid: ([0-9a-f]{32})", head, re.M)
        if m and m.group(1) in targets:
            print(f"{targets[m.group(1)]}  ->  {p[:-5]}")
