# eye_compare.py - register rebuild -> original with mfg_reg3 (pupils), resample the original's eye
# boxes into rebuild pixels, and report per-eye luminance correlation, mean abs diff, dark-blob count.
# Writes a side-by-side zoom (original resampled | rebuild) per frame.
# Usage: python eye_compare.py ORIG REB [OUT.png]
import sys, numpy as np
from PIL import Image
sys.path.insert(0, 'C:/mfgtmp/claude')
import mfg_reg3 as R
exec(open('C:/mfgtmp/claude/scripts/pupils.py').read().split(chr(10)+'def find')[0])

o = R.load(sys.argv[1]); r = R.load(sys.argv[2])
oL, oR = R.pupil(o, R.HINT_ORIG[0]), R.pupil(o, R.HINT_ORIG[1])
rL, rR = R.pupil(r, R.HINT_REB[0]), R.pupil(r, R.HINT_REB[1])
M, t, s, ang = R.similarity(rL[:2], rR[:2], oL[:2], oR[:2])

def bilinear(img, x, y):
    x0 = np.clip(np.floor(x).astype(int), 0, img.shape[1] - 2); y0 = np.clip(np.floor(y).astype(int), 0, img.shape[0] - 2)
    fx = (x - x0)[..., None]; fy = (y - y0)[..., None]
    return (img[y0, x0] * (1 - fx) * (1 - fy) + img[y0, x0 + 1] * fx * (1 - fy)
            + img[y0 + 1, x0] * (1 - fx) * fy + img[y0 + 1, x0 + 1] * fx * fy)

# rebuild-pixel boxes around each rebuild pupil
tiles = []
for name, (cx, cy) in (('eye_L', rL[:2]), ('eye_R', rR[:2])):
    ys, xs = np.mgrid[int(cy) - 16:int(cy) + 14, int(cx) - 13:int(cx) + 13]
    pts = np.stack([xs, ys], -1).astype(float)
    op = pts @ M.T + t
    oc = bilinear(o, op[..., 0], op[..., 1]); rc = r[ys, xs]
    lo, lr = oc.mean(-1), rc.mean(-1)
    corr = np.corrcoef(lo.ravel(), lr.ravel())[0, 1]
    mad = np.abs(oc - rc).mean()
    dk = lambda l: label(l < 70)[1]
    print(f'{name}: lum corr {corr:.3f}  mean|dRGB| {mad:.1f}  mean lum orig {lo.mean():.0f} reb {lr.mean():.0f}  '
          f'dark blobs orig {dk(lo)} reb {dk(lr)}  dark px orig {(lo<70).sum()} reb {(lr<70).sum()}')
    tiles.append(np.concatenate([oc, np.full((oc.shape[0], 2, 3), 255), rc], 1))
if len(sys.argv) > 3:
    im = np.concatenate([tiles[0], np.full((tiles[0].shape[0], 4, 3), 255), tiles[1]], 1).clip(0, 255).astype(np.uint8)
    Image.fromarray(im).resize((im.shape[1] * 8, im.shape[0] * 8), Image.NEAREST).save(sys.argv[3])
