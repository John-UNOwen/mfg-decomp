# Predict screen rows of the cell-12 smile ink under (a) the current *0.25 mapping and
# (b) the authored additive mapping, by rasterizing mouth triangles in uv space.
import sys, numpy as np
sys.path.insert(0, r"C:\mfgtmp\fresh\proj")
import project_mesh as PM
from PIL import Image
st = PM.standing3d_camera()
cam = PM.Cam(np.eye(4) @ st["trs"], st["fov"], PM.RT_W / PM.RT_H, ortho=bool(st["ortho"]),
             ortho_size=float(st["orthoSize"] or 5))
meshes = PM.load_meshes(PM.FBX_REBUILD_GLB)
pr = meshes["model_face_mouth"]; pr = pr[0] if isinstance(pr, list) else pr; print("keys", list(pr))
if pr is None: print(type(meshes), list(meshes)[:5]); sys.exit()
P = pr["attributes"]["POSITION"].astype(float) if isinstance(pr, dict) else None
uv = PM.glb_uv(pr, "TEXCOORD_0")      # unity convention (v flipped)
tri = pr["indices"].reshape(-1, 3) if "indices" in pr else None
sp = cam.project(P)                    # RT px (x, y, depth?)
print('uv range', uv.min(0), uv.max(0), 'rt rows', sp[:, 1].min(), sp[:, 1].max())
MSK = np.array(Image.open(r"C:/mfg/build/unity-project/Assets/MFG/Characters/Character1/3D/Common/mdl_mouth_msk.png").convert("RGBA")).astype(float) / 255
cap = lambda rt: 293 + (rt - 576) * (43 / 85)
def ink_rows(mapf, label):
    # rasterize each triangle in texel space of a 1024 atlas; record RT row of texels on ink
    hits = []
    for t in tri:
        a_,b_=uv[t[1]]-uv[t[0]],uv[t[2]]-uv[t[0]]
        if abs(a_[0]*b_[1]-a_[1]*b_[0]) < 1e-9: continue
        A = mapf(uv[t])                         # atlas uv (u, v bottom-up)
        tx = A[:, 0] * 1024; ty = (1 - A[:, 1]) * 1024   # texel col, row (top-down)
        x0, x1 = int(np.floor(tx.min())), int(np.ceil(tx.max()))
        y0, y1 = int(np.floor(ty.min())), int(np.ceil(ty.max()))
        Mi = np.linalg.inv(np.array([[tx[1]-tx[0], tx[2]-tx[0]], [ty[1]-ty[0], ty[2]-ty[0]]]))
        for yy in range(max(y0, 931), min(y1, 963)):
            for xx in range(max(x0, 89), min(x1, 173)):
                l = Mi @ np.array([xx + .5 - tx[0], yy + .5 - ty[0]])
                if l.min() >= -1e-6 and l.sum() <= 1 + 1e-6:
                    w = np.array([1 - l.sum(), l[0], l[1]])
                    p = (w[:, None] * sp[t]).sum(0)
                    if MSK[yy, xx, 0] > 0.5: hits.append((yy, xx, p[0], p[1]))
    if not hits: print(label, 'NO ink coverage'); return
    h = np.array(hits)
    print(f'{label}: {len(h)} ink texels covered; RT rows {h[:,3].min():.1f}-{h[:,3].max():.1f} '
          f'-> capture rows {cap(h[:,3].min()):.1f}-{cap(h[:,3].max()):.1f}; texel rows {int(h[:,0].min())}-{int(h[:,0].max())}')
    for c0,c1 in ((95,110),(120,140),(150,165)):
        m=h[(h[:,1]>=c0)&(h[:,1]<c1)]
        if len(m): print(f'   texel cols {c0}-{c1}: capture rows {cap(m[:,3].min()):.1f}-{cap(m[:,3].max()):.1f}')
    return h
ink_rows(lambda q: np.c_[q[:, 0], q[:, 1] * 0.25], 'current  v=uv.y*0.25+0')
ink_rows(lambda q: np.c_[q[:, 0], q[:, 1] * 0.25 - 0.145], 'debug2   v=uv.y*0.25-0.145')
ink_rows(lambda q: np.c_[q[:, 0], q[:, 1] - 0.75], 'authored v=uv.y-0.75')
