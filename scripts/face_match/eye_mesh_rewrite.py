# eye_mesh_rewrite.py - replace a Unity eye mesh's vertex streams + index buffer with the original
# glb data (same vertex count). glb->Unity: x mirrored (pos, normal, tangent.x, tangent.w flipped),
# v flipped, winding flipped. Bone stream (single bone index) kept as is.
import sys, re, struct, numpy as np
sys.path.insert(0, 'C:/mfgtmp/claude/scripts')
from glb_dump import load, acc as gacc

def rewrite(asset, glb, write):
    t = open(asset, newline='').read()
    i0 = t.index('m_VertexData:')
    n = int(re.search(r'm_VertexCount: (\d+)', t[i0:]).group(1))
    m = re.search(r'(_typelessdata: )([0-9a-f]+)', t[i0:])
    old = bytes.fromhex(m.group(2))
    js, b = load(glb); pr = js['meshes'][0]['primitives'][0]; at = pr['attributes']
    G = {k: np.array(gacc(js, b, at[k]), dtype=np.float64) for k in at}
    assert len(G['POSITION']) == n
    P = G['POSITION'] * [-1, 1, 1]; N = G['NORMAL'] * [-1, 1, 1]
    T4 = G['TANGENT'] * [-1, 1, 1, -1]
    s0 = np.c_[P, N, T4].astype('<f4').tobytes()
    s1 = np.hstack([np.c_[G['TEXCOORD_%d' % k][:, 0], 1 - G['TEXCOORD_%d' % k][:, 1]] for k in range(4)]).astype('<f4').tobytes()
    pad = lambda x: x + b'\0' * ((-len(x)) % 16)
    s0p = pad(s0); s1p = pad(s1)
    s2 = old[len(s0p) + len(s1p):]
    assert len(s2) == 4 * n, (len(old), len(s0p), len(s1p))
    new = s0p + s1p + s2
    assert len(new) == len(old)
    gi = np.array(gacc(js, b, pr['indices'])).astype(int).reshape(-1, 3)[:, [0, 2, 1]]
    ib_new = gi.astype('<u2').tobytes().hex()
    mi = re.search(r'(m_IndexBuffer: )([0-9a-f]+)', t)
    assert len(mi.group(2)) == len(ib_new)
    # sanity: old positions as a set == new positions as a set
    oldP = np.frombuffer(old[:40 * n], '<f4').reshape(n, 10)[:, :3]
    d = np.sqrt(((oldP[:, None] - P[None]) ** 2).sum(-1)).min(1).max()
    print(asset.split('Characters/')[-1], 'pos set err', d, 'bytes', len(new), 'index hex', len(ib_new))
    t2 = t[:mi.start(2)] + ib_new + t[mi.end(2):]
    j0 = t2.index('m_VertexData:'); m2 = re.search(r'(_typelessdata: )([0-9a-f]+)', t2[j0:])
    t2 = t2[:j0 + m2.start(2)] + new.hex() + t2[j0 + m2.end(2):]
    if write:
        open(asset, 'w', newline='').write(t2)
        print('  written')

if __name__ == '__main__':
    # usage: python eye_mesh_rewrite.py [--write] CharacterN/3D/C00X [...]
    w = '--write' in sys.argv
    for rel in [a for a in sys.argv[1:] if not a.startswith('--')] or ['Character1/3D/C001']:
        base = 'C:/mfg/build/unity-project/Assets/MFG/Characters/%s/Fbxs/' % rel
        gb = 'C:/mfg/build/export-primary-content/Assets/MFG/Characters/%s/Fbxs/' % rel
        for s in 'LR':
            rewrite(base + 'model_face_eye_%s.asset' % s, gb + 'model_face_eye_%s.glb' % s, w)
