# Offline render of model_face_eye_L with the original eye composite (eye_asm08 316-336).
import sys, numpy as np
from glb_dump import load, acc
from PIL import Image
EYE_IDX = int(sys.argv[1]) if len(sys.argv) > 1 else 0
PUP = sys.argv[2] if len(sys.argv) > 2 else 'raw'
C = r'C:\mfg\build\unity-project\Assets\MFG\Characters\Character1\3D\Common'
js, b = load(r'C:\mfg\build\export-primary-content\Assets\MFG\Characters\Character1\3D\C001\Fbxs\model_face_eye_L.glb')
pr = js['meshes'][0]['primitives'][0]; p = pr['attributes']; T = acc(js, b, pr['indices']).astype(int).reshape(-1, 3)
P = acc(js, b, p['POSITION'])
def U(k):
    a = acc(js, b, p[k]).copy(); a[:, 1] = 1 - a[:, 1]; return a
uv0, uv1, uv3 = U('TEXCOORD_0'), U('TEXCOORD_1'), U('TEXCOORD_3')
def tex(name):
    return np.array(Image.open(C + chr(92) + name).convert('RGBA')).astype(float) / 255
art, msk, pup, face = tex('mdl_eye.png'), tex('mdl_eye_msk.png'), tex('mdl_pupil_L.png'), tex('mdl_face_shy.png')
def samp(t, uv, wrap=False):
    h, w = t.shape[:2]; u, v = uv[..., 0], uv[..., 1]
    if wrap: u, v = u % 1.0, v % 1.0
    x = np.clip((u * w).astype(int), 0, w - 1); y = np.clip(((1 - v) * h).astype(int), 0, h - 1)
    return t[y, x]
cell = np.array([0.25 * (EYE_IDX % 4), -0.25 * (EYE_IDX // 4)])
W, H = 360, 300
xs, ys = P[:, 0], P[:, 1]
sx = lambda x: (x - xs.min()) / (xs.max() - xs.min()) * (W - 1)
sy = lambda y: (ys.max() - y) / (ys.max() - ys.min()) * (H - 1)
img = np.zeros((H, W, 3)); zb = np.full((H, W), -1e9)
for t in T:
    X = sx(xs[t]); Y = sy(ys[t]); Z = P[t, 2]
    x0, x1 = int(max(0, np.floor(X.min()))), int(min(W - 1, np.ceil(X.max())))
    y0, y1 = int(max(0, np.floor(Y.min()))), int(min(H - 1, np.ceil(Y.max())))
    d = (X[1] - X[0]) * (Y[2] - Y[0]) - (X[2] - X[0]) * (Y[1] - Y[0])
    if abs(d) < 1e-9: continue
    gx, gy = np.meshgrid(np.arange(x0, x1 + 1) + .5, np.arange(y0, y1 + 1) + .5)
    l1 = ((gx - X[0]) * (Y[2] - Y[0]) - (X[2] - X[0]) * (gy - Y[0])) / d
    l2 = ((X[1] - X[0]) * (gy - Y[0]) - (gx - X[0]) * (Y[1] - Y[0])) / d
    l0 = 1 - l1 - l2; m = (l0 >= 0) & (l1 >= 0) & (l2 >= 0)
    if not m.any(): continue
    w = np.stack([l0[m], l1[m], l2[m]], 1)
    z = w @ Z; yy, xx = gy[m].astype(int), gx[m].astype(int)
    front = z > zb[yy, xx]
    if not front.any(): continue
    w, yy, xx, z = w[front], yy[front], xx[front], z[front]
    e = w @ uv0[t] + cell; f = w @ uv1[t]; q = w @ uv3[t]
    if PUP == 'legacy':   # rebuild winner path: frac(0.5 + 0.87u, 0.16 + 0.87v)
        q = np.stack([0.5 + 0.87 * q[:, 0], 0.16 + 0.87 * q[:, 1]], 1)
    M = samp(msk, e); A = samp(art, e); Pp = samp(pup, q, wrap=True); F = samp(face, f)
    layer = A[:, :3] * (1 - M[:, 1:2]) + Pp[:, :3] * M[:, 1:2]
    a = np.maximum(M[:, 1], M[:, 0])[:, None]
    col = F[:, :3] * (1 - a) + layer * a
    img[yy, xx] = col; zb[yy, xx] = z
Image.fromarray((img * 255).astype(np.uint8)).resize((W * 2, H * 2), Image.NEAREST).save(
    rf'C:\mfgtmp\claude\eye_offline_idx{EYE_IDX}_{PUP}.png')
print('saved')
