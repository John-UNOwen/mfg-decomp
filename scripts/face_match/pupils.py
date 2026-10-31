import numpy as np, sys
from PIL import Image
def label(m):
    h,w=m.shape; lab=np.zeros((h,w),int); n=0
    for y in range(h):
        for x in range(w):
            if m[y,x] and not lab[y,x]:
                n+=1; st=[(y,x)]; lab[y,x]=n
                while st:
                    cy,cx=st.pop()
                    for dy,dx in ((1,0),(-1,0),(0,1),(0,-1)):
                        ny,nx=cy+dy,cx+dx
                        if 0<=ny<h and 0<=nx<w and m[ny,nx] and not lab[ny,nx]:
                            lab[ny,nx]=n; st.append((ny,nx))
    return lab,n
def find(path, box, thr):
    a=np.array(Image.open(path).convert('RGB')).astype(int)
    x0,y0,x1,y1=box; lum=a[y0:y1,x0:x1].mean(2)
    lab,n=label(lum<thr); out=[]
    for i in range(1,n+1):
        ys,xs=np.nonzero(lab==i)
        if len(xs)<6: continue
        out.append((len(xs),round(xs.mean()+x0,1),round(ys.mean()+y0,1),'x',xs.min()+x0,xs.max()+x0,'y',ys.min()+y0,ys.max()+y0))
    out.sort(reverse=True); return out[:6]
print('orig',*find(r'C:\mfgtmp\ref_original_charselect.png',(230,330,460,470),45),sep='\n ')
for f in sys.argv[1:]:
    print(f,*find(f,(150,170,330,290),60),sep='\n ')
