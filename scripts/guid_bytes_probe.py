#!/usr/bin/env python3
# guid_bytes_probe.py - verify that .meta guids AssetRipper wrote for loose plugin dlls
# and exported .cs scripts appear verbatim (or byte-group-swapped) inside the READ-ONLY
# install's serialized files. Evidence for "script references survive loose-dll -> package
# swaps" (docs/21). Install opened read-only.
import sys, os, re

INSTALL = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data"
SEARCH_FILES = [
    "globalgamemanagers", "globalgamemanagers.assets", "resources.assets",
    "level0", "level0.resS", "level1", "sharedassets0.assets",
]

def variants(hexstr: bytes):
    raw = bytes.fromhex(hexstr.decode())
    v = {"raw": raw}
    # 4-byte group reverse within each uint32 (LE swap per group)
    s = b"".join(raw[i:i+4][::-1] for i in range(0, 16, 4))
    v["groups4"] = s
    v["reverse16"] = raw[::-1]
    # swap two 8-byte halves
    v["halves8"] = raw[8:] + raw[:8]
    # reverse within each 8-byte half
    v["groups8"] = raw[:8][::-1] + raw[8:][::-1]
    return v

def main():
    repo = sys.argv[1]
    # collect guids from selected metas
    metas = {}
    for pat in [
        r"build\unity-project\Assets\Plugins\kamunity.dll.meta",
        r"build\unity-project\Assets\Plugins\Shirahama.dll.meta",
        r"build\unity-project\Assets\Plugins\Unity.Addressables.dll.meta",
        r"build\unity-project\Assets\Plugins\Unity.ResourceManager.dll.meta",
        r"build\unity-project\Assets\Plugins\Unity.TextMeshPro.dll.meta",
        r"build\unity-project\Assets\Plugins\Unity.Timeline.dll.meta",
        r"build\unity-project\Assets\Plugins\UnityEngine.UI.dll.meta",
        r"build\unity-project\Assets\Plugins\Unity.RenderPipelines.Core.Runtime.dll.meta",
        r"build\unity-project\Assets\Plugins\Unity.AI.Navigation.dll.meta",
        r"build\unity-project\Assets\Plugins\Unity.Recorder.dll.meta",
        r"build\unity-project\Assets\Plugins\Autodesk.Fbx.dll.meta",
        r"build\unity-project\Assets\Plugins\UniRx.dll.meta",
        r"build\unity-project\Assets\Plugins\UniTask.dll.meta",
        r"build\unity-project\Assets\Plugins\DOTween.dll.meta",
        r"build\unity-project\Assets\Plugins\Springbone.Runtime.dll.meta",
        r"build\unity-project\Assets\Plugins\zxing.unity.dll.meta",
        r"build\unity-project\Assets\Plugins\Unity_userfully.dll.meta",
        r"build\unity-project\Assets\Plugins\Win10actLog-Runtime.dll.meta",
        r"build\unity-project\Assets\Plugins\VirtualSurround.dll.meta",
    ]:
        p = os.path.join(repo, pat)
        m = re.search(rb"guid: (\w{32})", open(p, "rb").read())
        metas[os.path.basename(pat)] = m.group(1)
    # a few exported .cs metas
    cs_samples = []
    for rel in [
        r"build\unity-project\Assets\Scripts\Assembly-CSharp\AccelDoraAnimationEventNotifier.cs.meta",
        r"build\unity-project\Assets\Plugins\Assembly-CSharp-firstpass\USnd\AudioManager.cs.meta",
        r"build\unity-project\Assets\Plugins\Assembly-CSharp-firstpass\TCSIO\BoardBase.cs.meta",
    ]:
        p = os.path.join(repo, rel)
        if not os.path.exists(p):
            continue
        m = re.search(rb"guid: (\w{32})", open(p, "rb").read())
        if m:
            cs_samples.append((rel.split("\\")[-1], m.group(1)))

    files = {}
    for name in SEARCH_FILES:
        p = os.path.join(INSTALL, name)
        if os.path.exists(p):
            files[name] = open(p, "rb").read()
        elif os.path.exists(p + ".assets"):
            files[name + ".assets"] = open(p + ".assets", "rb").read()
        elif os.path.exists(p.replace(".assets", "")):
            files[name] = open(p.replace(".assets", ""), "rb").read()

    print(f"install files loaded: {[(n, len(b)) for n, b in files.items()]}")
    results = {}
    for label, hexg in list(metas.items()) + cs_samples:
        hits = {}
        for vname, needle in variants(hexg).items():
            total = 0
            where = []
            for fname, data in files.items():
                c = 0
                start = 0
                while True:
                    i = data.find(needle, start)
                    if i < 0:
                        break
                    c += 1
                    start = i + 1
                    if c > 500:
                        break
                if c:
                    total += c
                    where.append(f"{fname}:{c}")
            if total:
                hits[vname] = (total, where[:4])
        results[hexg.decode() if isinstance(hexg, bytes) else hexg] = (label, hits)
        status = "MISS"
        for vn, (t, w) in hits.items():
            status = f"HIT {vn} x{t} in {w}"
            break
        print(f"{label:55} guid={hexg.decode() if isinstance(hexg, bytes) else hexg}  {status}")

    summary = {"results": {k: (l, h) for k, (l, h) in results.items()}}
    out = os.path.join(repo, "build", "project-merge-analysis", "guid_bytes_probe.json")
    import json
    json.dump(summary, open(out, "w"), indent=1)
    print("wrote", out)

if __name__ == "__main__":
    main()
