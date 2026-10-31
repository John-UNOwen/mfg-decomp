import numpy as np
from PIL import Image
def an(path,label,rows):
    a=np.array(Image.open(path).convert('RGB')).astype(int); R,G,B=a[...,0],a[...,1],a[...,2]
    crim=(R>140)&(G<70)&(B<80)&(R-G>90)
    ys,xs=np.nonzero(crim[:int(a.shape[0]*0.75)])
    print('==',label,a.shape)
    # bow: largest crimson component near center — report per-row extent
    for y in rows:
        xx=np.nonzero(crim[y])[0]
        if len(xx): print(f'  row {y}: crimson x {xx.min()}..{xx.max()} width {xx.max()-xx.min()} n {len(xx)}')
    # widest crimson row
    w=[(np.ptp(np.nonzero(crim[y])[0]) if crim[y].sum()>20 else 0) for y in range(a.shape[0])]
    y=int(np.argmax(w)); xx=np.nonzero(crim[y])[0]
    print('  widest crimson row',y,'x',xx.min(),xx.max(),'w',w[y])
    top=[y for y in range(a.shape[0]) if crim[y].sum()>20][:1]; print('  first crimson row(>20px)',top)
an(r'C:\mfgtmp\ref_original_charselect.png','orig',range(490,600,10))
an(r'C:\mfgtmp\prev_round\r0_blush_hold_01.png','reb',range(330,400,6))
