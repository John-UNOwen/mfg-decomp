import sys,re,numpy as np
FMT={0:('<f4',4),1:('<f2',2),2:('u1',1),4:('<u2',2),6:('u1',1),8:('<u2',2),10:('<u4',4)}
def decode(path):
    t=open(path).read()
    vd=t[t.index('m_VertexData:'):]
    n=int(re.search(r'm_VertexCount: (\d+)',vd).group(1))
    ch=re.findall(r'- stream: (\d+)\s+offset: (\d+)\s+format: (\d+)\s+dimension: (\d+)',vd)
    ch=[tuple(map(int,c)) for c in ch]
    b=bytes.fromhex(re.search(r'_typelessdata: ([0-9a-f]+)',vd).group(1))
    # stream strides
    strides={}
    for s,o,f,d in ch:
        if d: strides[s]=max(strides.get(s,0),o+FMT[f][1]*d)
    base={}; acc=0
    for s in sorted(strides):
        base[s]=acc; acc+=strides[s]*n
        acc=(acc+15)//16*16 if False else acc
    out={}
    for i,(s,o,f,d) in enumerate(ch):
        if not d: continue
        dt,sz=FMT[f]; a=np.zeros((n,d))
        for k in range(n):
            st=base[s]+k*strides[s]+o
            a[k]=np.frombuffer(b[st:st+sz*d],dt)
        out[i]=a
    return out,n,len(b),acc
if __name__=='__main__':
    o,n,L,acc=decode(sys.argv[1]); print('n',n,'bytes',L,'computed',acc)
    for k,v in o.items(): print(k,v.shape,v.min(0).round(4),v.max(0).round(4))
    if 12 in o and 13 in o:
        pairs={}
        for w,j in zip(o[12],o[13]):
            for ww,jj in zip(w,j):
                if ww>0: pairs[int(jj)]=pairs.get(int(jj),0)+ww
        print('weight per bone',{k:round(v,2) for k,v in sorted(pairs.items())})
