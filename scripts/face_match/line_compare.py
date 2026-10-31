# line_compare.py - smile line in ORIGINAL-frame coords: resample the rebuild through the mfg_reg3
# transform, then per original column (330..370) find the darkest row in 448..468 and its depth
# below the column's local skin median. Usage: python line_compare.py ORIG REB [REB...] [--img out.png]
import sys, numpy as np
from PIL import Image
sys.path.insert(0, 'C:/mfgtmp/claude')
import mfg_reg3 as R
args = [a for a in sys.argv[1:] if not a.startswith('--')]
img = sys.argv[sys.argv.index('--img') + 1] if '--img' in sys.argv else None
if img: args.remove(img)
o = R.load(args[0]); oL, oR = R.pupil(o, R.HINT_ORIG[0]), R.pupil(o, R.HINT_ORIG[1])
X0, X1, Y0, Y1 = 325, 376, 446, 470
def bil(a, x, y):
    x0 = np.floor(x).astype(int); y0 = np.floor(y).astype(int); fx = (x - x0)[..., None]; fy = (y - y0)[..., None]
    return a[y0, x0]*(1-fx)*(1-fy) + a[y0, x0+1]*fx*(1-fy) + a[y0+1, x0]*(1-fx)*fy + a[y0+1, x0+1]*fx*fy
def stats(a, tag):
    l = a.mean(-1); d = []; rows = []
    for x in range(330 - X0, 371 - X0, 2):
        col = l[:, x]; y = int(col.argmin()); d.append(np.median(col) - col[y]); rows.append(Y0 + y)
    d = np.array(d)
    print(f'  {tag}: depth median {np.median(d):.0f} mean {d.mean():.0f} max {d.max():.0f}; rows {min(rows)}-{max(rows)}; '
          f'darkest-pixel mean rgb {tuple(np.array([a[r-Y0, x] for r, x in zip(rows, range(330-X0, 371-X0, 2))]).mean(0).round().astype(int))}')
    return d
ys, xs = np.mgrid[Y0:Y1, X0:X1]
stats(o[Y0:Y1, X0:X1], 'original')
tiles = [o[Y0:Y1, X0:X1]]
for p in args[1:]:
    r = R.load(p); rL, rR = R.pupil(r, R.HINT_REB[0]), R.pupil(r, R.HINT_REB[1])
    M, t, s, ang = R.similarity(rL[:2], rR[:2], oL[:2], oR[:2]); Mi = np.linalg.inv(M)
    rp = (np.stack([xs, ys], -1).astype(float) - t) @ Mi.T
    a = bil(r, rp[..., 0], rp[..., 1]); stats(a, p.split('/')[-2] + '/' + p.split('/')[-1]); tiles.append(a)
if img:
    im = np.concatenate([np.concatenate([t_, np.full((t_.shape[0], 2, 3), 255)], 1) for t_ in tiles], 1).clip(0, 255).astype(np.uint8)
    Image.fromarray(im).resize((im.shape[1]*8, im.shape[0]*8), Image.NEAREST).save(img)
