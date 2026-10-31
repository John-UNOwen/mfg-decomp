# eye_uvfix.py - recover per-vertex uv1..uv3 of the Unity eye mesh from the original glb by
# matching triangles (corner pos+uv0), and report/emit consistent per-vertex values.
import sys, re, numpy as np
sys.path.insert(0, 'C:/mfgtmp/claude/scripts')
from glb_dump import load, acc as gacc
src = open('C:/mfgtmp/claude/scripts/asset_mesh2.py').read().replace(
    "acc=(acc+15)//16*16 if False else acc", "acc=(acc+15)//16*16")
ns = {'__name__': 'x'}; exec(src, ns)

def analyse(asset, glb):
    o, n, L, _ = ns['decode'](asset)
    t = open(asset).read()
    ib = np.frombuffer(bytes.fromhex(re.search(r'm_IndexBuffer: ([0-9a-f]+)', t).group(1)), '<u2').reshape(-1, 3)
    js, b = load(glb)
    pr = js['meshes'][0]['primitives'][0]; at = pr['attributes']
    G = {k: np.array(gacc(js, b, at[k])) for k in at}
    gi = np.array(gacc(js, b, pr['indices'])).astype(int).reshape(-1, 3)
    gp = G['POSITION'] * [-1, 1, 1]
    T = {k: np.c_[G['TEXCOORD_%d' % k][:, 0], 1 - G['TEXCOORD_%d' % k][:, 1]] for k in range(4)}
    kU = np.c_[o[0], o[4]]; kG = np.c_[gp, T[0]]
    # glb triangle lookup by sorted rounded corner keys
    def tk(K, tri): return tuple(sorted(tuple(np.round(K[v], 4)) for v in tri))
    gl = {}
    for tri in gi: gl.setdefault(tk(kG, tri), []).append(tri)
    need = {v: [] for v in range(n)}; miss = 0
    for tri in ib:
        cands = gl.get(tk(kU, tri))
        if not cands: miss += 1; continue
        g = cands[0]
        for v in tri:
            # corner of g with same key
            j = [w for w in g if np.allclose(np.round(kG[w], 4), np.round(kU[v], 4))][0]
            need[v].append(np.r_[T[1][j], T[2][j], T[3][j]])
    incons = 0; out = np.c_[o[5], o[6], o[7]].copy()
    for v, L_ in need.items():
        if not L_: continue
        A = np.array(L_)
        if np.abs(A - A[0]).max() > 1e-4: incons += 1
        out[v] = A[0]
    cur = np.c_[o[5], o[6], o[7]]
    print(asset, 'tris', len(ib), 'glb tris', len(gi), 'unmatched', miss, 'inconsistent verts', incons,
          'verts changed', (np.abs(out - cur).max(1) > 1e-4).sum())
    return o, out, n

if __name__ == '__main__':
    base = 'C:/mfg/build/unity-project/Assets/MFG/Characters/Character1/3D/C001/Fbxs/'
    gb = 'C:/mfg/build/export-primary-content/Assets/MFG/Characters/Character1/3D/C001/Fbxs/'
    for s in 'LR':
        analyse(base + 'model_face_eye_%s.asset' % s, gb + 'model_face_eye_%s.glb' % s)
