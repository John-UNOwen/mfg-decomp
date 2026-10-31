import os
import UnityPy

BD = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64"
B = "02085b48d0e488bd50a6cf528b0323e7.bundle"
env = UnityPy.load(os.path.join(BD, B))
for obj in env.objects:
    if getattr(obj.type, "name", "?") != "Mesh":
        continue
    n = obj.read()
    if str(getattr(n, "m_Name", "")) != "model_face_mouth":
        continue
    vd = n.m_VertexData
    print("vertcount:", getattr(n, "m_VertexCount", None), getattr(vd, "m_VertexCount", None))
    print("vertexdata attrs:", [a for a in dir(vd) if not a.startswith("_")][:30])
    chs = getattr(vd, "m_VertexChannels", None) or getattr(vd, "m_Channels", None)
    for i, c in enumerate(chs):
        print(i, c)
    print("datasize:", getattr(vd, "m_DataSize", None))
    d = getattr(vd, "m_Data", None)
    print("m_Data type:", type(d).__name__, getattr(d, "m_Size", None))
