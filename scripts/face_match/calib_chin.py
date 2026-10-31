import sys, numpy as np
sys.path.insert(0, r"C:\mfgtmp\fresh\proj")
import project_mesh as PM
st = PM.standing3d_camera()
cam = PM.Cam(np.eye(4) @ st["trs"], st["fov"], PM.RT_W / PM.RT_H, ortho=bool(st["ortho"]), ortho_size=float(st["orthoSize"] or 5))
M = PM.load_meshes(PM.FBX_REBUILD_GLB); print(list(M))
for name in ("model_face", "model_face_mouth", "model_face_eye_L", "model_face_eye_R"):
    pr = M[name]; pr = pr[0] if isinstance(pr, list) else pr
    P = pr["attributes"]["POSITION"].astype(float); sp = cam.project(P)
    mid = np.abs(P[:, 0]) < 0.006
    print(f'{name:18s} RT rows {sp[:,1].min():.1f}-{sp[:,1].max():.1f}  cols {sp[:,0].min():.1f}-{sp[:,0].max():.1f}'
          f'  midline(|x|<6mm) rows {sp[mid,1].min():.1f}-{sp[mid,1].max():.1f}')
    if name == "model_face":
        # chin: lowest front-facing midline vertex (z>0 forward?) -> print a few lowest
        idx = np.argsort(-sp[:, 1])[:8]
        for i in idx: print('   low vtx', np.round(P[i], 4), 'rt', np.round(sp[i, :2], 1))
