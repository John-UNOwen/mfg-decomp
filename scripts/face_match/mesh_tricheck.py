# mesh_tricheck.py - does every Unity triangle exist in the glb with identical corner attributes?
# Usage: python mesh_tricheck.py ASSET GLB
import sys, re, numpy as np
sys.path.insert(0, 'C:/mfgtmp/claude/scripts')
from glb_dump import load, acc as gacc
src = open('C:/mfgtmp/claude/scripts/asset_mesh2.py').read().replace("acc=(acc+15)//16*16 if False else acc", "acc=(acc+15)//16*16")
ns = {'__name__': 'x'}; exec(src, ns)

def check(asset, glb):
    o, n, L, _ = ns['decode'](asset)
    t = open(asset).read()
    fmt = '<u2' if re.search(r'm_IndexFormat: (\d)', t).group(1) == '0' else '<u4'
    ib = np.frombuffer(bytes.fromhex(re.search(r'm_IndexBuffer: ([0-9a-f]+)', t).group(1)), fmt).reshape(-1, 3)
    js, b = load(glb)
    chans = [c for c in sorted(o) if 4 <= c <= 11]
    gk, gtris, off = [], [], 0
    for m in js['meshes']:
        for pr in m['primitives']:
            at = pr['attributes']
            P = np.array(gacc(js, b, at['POSITION'])) * [-1, 1, 1]
            cols = [P]
            for c in chans:
                a = np.array(gacc(js, b, at['TEXCOORD_%d' % (c - 4)]))
                cols.append(np.c_[a[:, 0], 1 - a[:, 1]])
            gk.append(np.hstack(cols))
            gtris.append(np.array(gacc(js, b, pr['indices'])).astype(int).reshape(-1, 3) + off)
            off += len(P)
    GK = np.round(np.vstack(gk), 3); GT = np.vstack(gtris)
    UK = np.round(np.hstack([o[0]] + [o[c] for c in chans]), 3)
    key = lambda K, tri: tuple(sorted(tuple(K[v]) for v in tri))
    gs = set(key(GK, tri) for tri in GT)
    miss = sum(key(UK, tri) not in gs for tri in ib)
    # position-only (topology) check
    gsp = set(key(GK[:, :3], tri) for tri in GT)
    missp = sum(key(UK[:, :3], tri) not in gsp for tri in ib)
    print(f'{asset.split("/")[-1]}: tris {len(ib)} vs {len(GT)}; uv chans {[c-4 for c in chans]}; '
          f'tris w/o exact glb twin {miss}; w/o position twin {missp}')

if __name__ == '__main__':
    check(sys.argv[1], sys.argv[2])
