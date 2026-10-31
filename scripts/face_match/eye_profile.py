import numpy as np, sys
from PIL import Image
def prof(path, cx_list, y0, y1, label):
    a=np.array(Image.open(path).convert('RGB')).astype(int)
    print('==',label)
    for cx in cx_list:
        print(' col',cx)
        for y in range(y0,y1):
            r,g,b=a[y,cx-1:cx+2].mean(0).astype(int); lum=(r+g+b)//3
            tag='DARK' if lum<80 else ('amber' if (r-b>55 and g-b>15 and g<190) else '')
            print(f'  {y} {r:3d},{g:3d},{b:3d} {lum:3d} {tag}')
prof(r'C:\mfgtmp\ref_original_charselect.png',[288],320,420,'orig iris L col')
prof(r'C:\mfgtmp\prev_round\r0_blush_hold_01.png',[211],190,262,'reb iris L col')
