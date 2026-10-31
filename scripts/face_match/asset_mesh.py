import sys,re,numpy as np
t=open(sys.argv[1]).read()
n=int(re.search(r'm_VertexCount: (\d+)',t).group(1))
hx=re.search(r'_typelessdata: ([0-9a-f]+)',t).group(1); b=bytes.fromhex(hx)
s0=b[:40*n]; s1=b[40*n:40*n+16*n]; s2=b[56*n:60*n]
P=np.frombuffer(s0,'<f4').reshape(n,10)[:,:3]
UV=np.frombuffer(s1,'<f4').reshape(n,4)
BI=np.frombuffer(s2,'<u4')
print('pos min',P.min(0).round(4),'max',P.max(0).round(4))
print('uv0 min',UV[:,:2].min(0).round(4),'max',UV[:,:2].max(0).round(4))
print('uv1 min',UV[:,2:].min(0).round(4),'max',UV[:,2:].max(0).round(4))
print('bone idx counts',dict(zip(*np.unique(BI,return_counts=True))))
np.save(sys.argv[2],np.hstack([P,UV,BI[:,None]]))
