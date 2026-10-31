p=r'C:\mfgtmp\claude\scripts\predict_ink.py'; s=open(p).read()
i=s.index('msk = np.array('); j=s.index('\n',i)
s=s[:i]+'MSK = np.array(Image.open(r"C:/mfg/build/unity-project/Assets/MFG/Characters/Character1/3D/Common/mdl_mouth_msk.png").convert("RGBA")).astype(float) / 255'+s[j:]
s=s.replace("                    hits.append((yy, xx, p[0], p[1]))","                    if MSK[yy, xx, 0] > 0.5: hits.append((yy, xx, p[0], p[1]))")
s=s.replace("    return h\n","    for c0,c1 in ((95,110),(120,140),(150,165)):\n        m=h[(h[:,1]>=c0)&(h[:,1]<c1)]\n        if len(m): print(f'   texel cols {c0}-{c1}: capture rows {cap(m[:,3].min()):.1f}-{cap(m[:,3].max()):.1f}')\n    return h\n")
open(p,'w').write(s)
