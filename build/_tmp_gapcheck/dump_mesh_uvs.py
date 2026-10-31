import os
import UnityPy

BD = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64"
HITS = """02085b48d0e488bd50a6cf528b0323e7 03d1c8dbb9a80003faa73f5ddd0e1e15 046765fd6b0758ce3f9f198a5008f2a8
04aca0d0691dd18d0cc444b787447234 0a250b2e64d4b0f870f1cc2e7d66e93f 1e659258e10a8361c8fe54fd3c625463
2b05e427f6841aa76b4b03f6c9073f4e 3dfad453cae395afd566133aef9d4ae6 4a0603618947dae11f0649891a086a7f
4abe7d91a61bafbb50e17e8b9119ffcd 4eca73b856b27ea95786f9cee85df57e 5005fb018f625f3651ca4ac49779b9fc
525cbc02ec2c99eda422160fbb3e55a1 58532ce6e33bfac47f3e7ffac24725ab 5ca10abbb876d2abac7332bd11f18d82
608313c8f4addcbbbcd0a293e6588bc6 62116ef2668e40ff47b0b4ebbab5bf94 67fc3122f9aa090a6516c21b69507236
6870647beabcde5d28333f3c191bee9b 6ff139bd5f171a6aca6518434cf48ff0 8e3447fbf993eab13e777d8a2a22e000
9749066f4ceb8603368d59b6851c3ea4 aff4cf449629a024f639b06a5e644300 c32ac8de19d83c9c7a2768ce9e7332db
c4788c29c0fa2ec4bead293f0f5fa0b7 cd487f365ac27ea296b1cb90ec5625cf cf1647ddad33d67889b21b12f1104875
d0021fdb6494b887d3e12d6d49e1d942 d1488ae76d10b6eaea46e8c49561f850 d209e07a1395d5b484d7508dd21b0324
d7359c0b36888bc21890a4b155995999 de0533b614dadb784f238fe5b9ff4813 e17abfac20f249994fe5bdb66a0dd746
e2277da4592d3c79e0191864f856b491 e293957ff59602d69dc7579173715703 efec5ecc4418a53a5c87281ee8184113
fa66202e82b7b613d24dd7d61ba5ddcd""".split()

def rng(seq):
    if not seq:
        return None
    xs = [p[0] for p in seq]
    ys = [p[1] for p in seq]
    return (round(min(xs), 4), round(min(ys), 4), round(max(xs), 4), round(max(ys), 4))

for b in HITS:
    try:
        env = UnityPy.load(os.path.join(BD, b + ".bundle"))
    except Exception as e:
        print(b, "load fail", repr(e)[:80])
        continue
    for obj in env.objects:
        if getattr(obj.type, "name", "?") != "Mesh":
            continue
        try:
            n = obj.read()
        except Exception as e:
            continue
        nm = str(getattr(n, "m_Name", ""))
        if "mouth" not in nm:
            continue
        cnt = getattr(n, "m_VertexCount", None)
        uvs = {}
        for ch in ("uv0", "uv1", "uv2", "uv3"):
            try:
                v = getattr(n, ch)
            except Exception:
                v = None
            uvs[ch] = rng(v) if v else None
        print(b[:10], nm, "verts:", cnt, "uvs:", uvs)
