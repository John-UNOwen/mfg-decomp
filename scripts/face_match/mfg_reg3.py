#!/usr/bin/env python
# mfg_reg3.py - rebuild -> original registration anchored on the PUPILS.
# Fixes mfg_reg2.py, whose iris hints (orig ~(291,371)/(393,371), rebuild ~(213,227)/(280,227))
# sit on the bangs ~40/50 px above the real eyes; its amber mask also matches red hair.
# Usage: python mfg_reg3.py ORIG.PNG REB1.PNG [REB2 ...] [--json out.json]
import sys, math, json, argparse
import numpy as np
from PIL import Image

HINT_ORIG = ((320, 412), (386, 406))   # real pupils, original 1778x1026
HINT_REB = ((234, 277), (284, 275))    # real pupils, rebuild 1280x720

# Region boxes in ORIGINAL-frame pixels (x0, y0, x1, y1), picked on the original's anatomy.
REGIONS = {
    'between_eyes': (345, 422, 360, 434),
    'nose':         (335, 428, 355, 446),
    'cheek_L':      (296, 432, 324, 452),
    'cheek_R':      (378, 426, 406, 446),
    'mouth_up':     (330, 447, 370, 455),
    'mouth_line':   (330, 455, 370, 462),
    'chin':         (332, 464, 368, 478),
    'neck':         (335, 483, 355, 491),
    'eye_L':        (305, 396, 334, 428),
    'eye_R':        (372, 390, 400, 422),
    'face':         (300, 420, 405, 480),
}

def load(p):
    return np.array(Image.open(p).convert('RGB')).astype(float)

def pupil(a, hint, rad=14, thr=60):
    """Centroid of the darkest pixels (lum < thr) near the hint, weighted by darkness."""
    cx, cy = hint
    y0, y1, x0, x1 = cy - rad, cy + rad, cx - rad, cx + rad
    lum = a[y0:y1, x0:x1].mean(2)
    w = np.clip(thr - lum, 0, None)
    if w.sum() == 0:
        raise RuntimeError(f'no dark pixels near {hint}')
    ys, xs = np.mgrid[y0:y1, x0:x1]
    return float((xs * w).sum() / w.sum()), float((ys * w).sum() / w.sum()), int((w > 0).sum())

def similarity(fL, fR, tL, tR):
    fL, fR, tL, tR = map(lambda p: np.array(p, float), (fL, fR, tL, tR))
    vf, vt = fR - fL, tR - tL
    s = np.hypot(*vt) / np.hypot(*vf)
    ang = math.atan2(vt[1], vt[0]) - math.atan2(vf[1], vf[0])
    M = s * np.array([[math.cos(ang), -math.sin(ang)], [math.sin(ang), math.cos(ang)]])
    return M, tL - M @ fL, s, math.degrees(ang)

def midline_events(a, x, y0, y1):
    """Rows along a column: first neck-strip row (214,124,102 +-12), first crimson row."""
    neck = crim = None
    for y in range(y0, y1):
        r, g, b = a[y, x - 1:x + 2].mean(0)
        if neck is None and abs(r - 214) < 12 and abs(g - 124) < 12 and abs(b - 102) < 12:
            neck = y
        if crim is None and r > 170 and g < 70 and b < 70:
            crim = y
    return neck, crim

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('orig'); ap.add_argument('reb', nargs='+'); ap.add_argument('--json')
    args = ap.parse_args()
    o = load(args.orig)
    oL, oR = pupil(o, HINT_ORIG[0]), pupil(o, HINT_ORIG[1])
    o_neck, o_crim = midline_events(o, 345, 440, 560)
    print(f'ORIGINAL pupils L=({oL[0]:.1f},{oL[1]:.1f}) R=({oR[0]:.1f},{oR[1]:.1f}) '
          f'IOD={math.hypot(oR[0]-oL[0], oR[1]-oL[1]):.2f}  neck@x345={o_neck} crimson@x345={o_crim}')
    out = []
    for p in args.reb:
        r = load(p)
        rL, rR = pupil(r, HINT_REB[0]), pupil(r, HINT_REB[1])
        M, t, s, ang = similarity(rL[:2], rR[:2], oL[:2], oR[:2])
        Mi = np.linalg.inv(M)
        to_reb = lambda x, y: Mi @ (np.array([x, y], float) - t)
        # verification on landmarks independent of the eyes
        rx = to_reb(345, 470)[0]
        r_neck, r_crim = midline_events(r, int(round(rx)), 290, 400)
        ver = {}
        for name, ry, oy in (('neck_top', r_neck, o_neck), ('crimson_top', r_crim, o_crim)):
            if ry is None or oy is None:
                ver[name] = None; continue
            mapped = (M @ np.array([rx, ry]) + t)[1]
            ver[name] = (ry, round(float(mapped), 1), oy, round(float(mapped - oy), 1))
        print(f'\n== {p}\n pupils L=({rL[0]:.1f},{rL[1]:.1f}) R=({rR[0]:.1f},{rR[1]:.1f}) '
              f'IOD={math.hypot(rR[0]-rL[0], rR[1]-rL[1]):.2f}')
        print(f' transform reb->orig: scale={s:.4f} rot={ang:+.2f}deg t=({t[0]:.1f},{t[1]:.1f})')
        for k, v in ver.items():
            print(f' verify {k:12s} reb_row={v[0]} mapped={v[1]} orig={v[2]} err={v[3]:+.1f}px' if v else f' verify {k}: not found')
        tgt = to_reb(345, 458)
        print(f' smile-line target (orig row 458, x 330-370) -> rebuild row {tgt[1]:.1f}, '
              f'cols {to_reb(330,458)[0]:.0f}-{to_reb(370,458)[0]:.0f}')
        print(f' {"region":13s} {"original RGB/lum":22s} {"rebuild RGB/lum":22s} delta')
        regs = {}
        for name, (x0, y0, x1, y1) in REGIONS.items():
            ys, xs = np.mgrid[y0:y1, x0:x1]
            pts = np.stack([xs.ravel(), ys.ravel()], 1).astype(float)
            rp = (Mi @ (pts - t).T).T
            rxi = np.clip(np.round(rp[:, 0]).astype(int), 0, r.shape[1] - 1)
            ryi = np.clip(np.round(rp[:, 1]).astype(int), 0, r.shape[0] - 1)
            oc = o[y0:y1, x0:x1].reshape(-1, 3).mean(0)
            rc = r[ryi, rxi].mean(0)
            d = rc - oc
            regs[name] = dict(orig=oc.round(1).tolist(), reb=rc.round(1).tolist(), delta=d.round(1).tolist())
            print(f' {name:13s} ({oc[0]:3.0f},{oc[1]:3.0f},{oc[2]:3.0f})/{oc.mean():3.0f}      '
                  f'({rc[0]:3.0f},{rc[1]:3.0f},{rc[2]:3.0f})/{rc.mean():3.0f}      '
                  f'({d[0]:+4.0f},{d[1]:+4.0f},{d[2]:+4.0f})/{d.mean():+4.0f}')
        out.append(dict(frame=p, pupils=[rL[:2], rR[:2]], scale=s, rot=ang, t=t.tolist(),
                        verify=ver, smile_target_row=float(tgt[1]), regions=regs))
    if args.json:
        json.dump(dict(orig_pupils=[oL[:2], oR[:2]], frames=out), open(args.json, 'w'), indent=1)

if __name__ == '__main__':
    main()
