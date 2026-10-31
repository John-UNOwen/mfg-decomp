# line_measure.py - find the smile line in rebuild captures (native 1280x720 px).
# Target (from mfg_reg3 / pupil+nose registration): row ~312-313, cols ~238-269.
# Per column: darkest row in [290, 340] and its depth vs the median of that column window.
# Usage: python line_measure.py FRAME.png [...]
import sys
import numpy as np
from PIL import Image

C0, C1, R0, R1 = 236, 272, 288, 342

for p in sys.argv[1:]:
    a = np.array(Image.open(p).convert('RGB')).astype(float)
    lum = a.mean(2)
    print(f'== {p}')
    rows = []
    for x in range(C0, C1, 3):
        col = lum[R0:R1, x]
        y = int(np.argmin(col)); depth = float(np.median(col) - col[y])
        rows.append((x, R0 + y, depth))
        rgb = a[R0 + y, x].astype(int)
        print(f'  col {x}: darkest row {R0 + y} lum {col[y]:.0f} depth {depth:.0f} rgb {tuple(rgb)}')
    # band census: rows where >= 8 of the sampled cols are >= 25 lum below their column median
    med = np.median(lum[R0:R1, C0:C1], axis=0)
    dark = (med[None, :] - lum[R0:R1, C0:C1]) >= 25
    cnt = dark.sum(1)
    band = [R0 + i for i, c in enumerate(cnt) if c >= 8]
    print(f'  dark-row census (>=8 of {C1 - C0} cols, >=25 below col median): {band}')
    if band:
        sel = a[band[0]:band[-1] + 1, C0:C1][dark[band[0] - R0:band[-1] - R0 + 1]]
        print(f'  mean dark-pixel rgb {tuple(sel.mean(0).round(0).astype(int))}  n={len(sel)}')
