import sys, numpy as np
from PIL import Image
a=np.array(Image.open(sys.argv[1]).convert('RGB')).astype(int); b=np.array(Image.open(sys.argv[2]).convert('RGB')).astype(int)
d=a.mean(2)-b.mean(2)   # new - base (negative = darker in new)
ys,xs=np.nonzero(np.abs(d)>=20)
print('pixels |dlum|>=20:',len(xs))
if len(xs):
    print(' bbox x',xs.min(),xs.max(),'y',ys.min(),ys.max())
    for y in range(ys.min(),ys.max()+1):
        row=np.nonzero(np.abs(d[y])>=20)[0]
        if len(row): print(f'  row {y}: n={len(row):3d} x {row.min()}-{row.max()} mean dlum {d[y,row].mean():+.0f} new rgb {tuple(a[y,row].mean(0).astype(int))} base {tuple(b[y,row].mean(0).astype(int))}')
