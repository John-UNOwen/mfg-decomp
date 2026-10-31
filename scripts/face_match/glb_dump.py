import sys, json, struct, numpy as np
def load(p):
    b=open(p,'rb').read(); assert b[:4]==b'glTF'
    off=12; js=None; bin_=None
    while off<len(b):
        ln,ty=struct.unpack_from('<II',b,off); ch=b[off+8:off+8+ln]
        if ty==0x4E4F534A: js=json.loads(ch)
        else: bin_=ch
        off+=8+ln
    return js,bin_
CT={5126:('f',4),5123:('H',2),5121:('B',1),5125:('I',4)}
NC={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4,'MAT4':16}
def acc(js,bin_,i):
    a=js['accessors'][i]; bv=js['bufferViews'][a['bufferView']]
    t,sz=CT[a['componentType']]; n=NC[a['type']]
    st=bv.get('byteStride',sz*n); base=bv.get('byteOffset',0)+a.get('byteOffset',0)
    out=np.zeros((a['count'],n))
    for k in range(a['count']):
        out[k]=struct.unpack_from('<'+t*n,bin_,base+k*st)
    if a.get('normalized'): out/= (255 if t=='B' else 65535)
    return out
if __name__=='__main__':
    js,bin_=load(sys.argv[1])
    print('nodes',len(js['nodes']),'meshes',len(js['meshes']),'skins',len(js.get('skins',[])))
    for m in js['meshes']:
        for p in m['primitives']:
            print(m.get('name'),'attrs',list(p['attributes']))
            for k,v in p['attributes'].items():
                d=acc(js,bin_,v); print(' ',k,d.shape,'min',d.min(0).round(4),'max',d.max(0).round(4))
    for s in js.get('skins',[]):
        names=[js['nodes'][j].get('name') for j in s['joints']]
        print('skin joints',len(names)); 
        ibm=acc(js,bin_,s['inverseBindMatrices'])
        p=js['meshes'][0]['primitives'][0]['attributes']
        J=acc(js,bin_,p['JOINTS_0']).astype(int); W=acc(js,bin_,p['WEIGHTS_0'])
        tot={}
        for j,w in zip(J.ravel(),W.ravel()):
            if w>0: tot[j]=tot.get(j,0)+w
        for j,w in sorted(tot.items(),key=lambda x:-x[1]):
            M=ibm[j].reshape(4,4).T
            print('  joint',j,names[j],'wsum',round(w,2),'ibm_t',M[:3,3].round(4))
