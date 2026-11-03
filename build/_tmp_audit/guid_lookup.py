import re, glob, os, json
# 1) find texture guid a90422c384cf0a548a05bacf5b794bf3 (+ each outline mat's texture guid)
root = r"C:\mfg\build\unity-project\Assets"
need = set()
for f in glob.glob(root+r"\MFG\Characters\Character1\3D\**\*.mat", recursive=True):
    t=open(f,encoding='utf-8',errors='replace').read()
    for m in re.finditer(r'm_Texture: \{fileID: (-?\d+), guid: ([0-9a-f]{32})', t):
        need.add(m.group(2))
found = {}
for meta in glob.glob(root+r"\**\*.meta", recursive=True):
    t=open(meta,encoding='utf-8',errors='replace').read(400)
    m=re.search(r'guid: ([0-9a-f]{32})',t)
    if m and m.group(1) in need:
        found[m.group(1)] = meta[:-5].replace(root+os.sep,"")
print("--- texture guids used by Character1 C001 outline mats + Common:")
for f in [root+r"\MFG\Characters\Character1\3D\Common\Mt_outline_hair.mat",
          root+r"\MFG\Characters\Character1\3D\Common\Mt_outline_face.mat",
          root+r"\MFG\Characters\Character1\3D\C001\Materials\Mt_outline_jacket.mat"]:
    t=open(f,encoding='utf-8',errors='replace').read()
    gs=set(re.findall(r'guid: ([0-9a-f]{32})',t))
    for g in gs:
        print(" ", os.path.basename(f), g, "->", found.get(g,"NOT FOUND"))
print("--- shader guid used by outline mats:")
for f in [root+r"\MFG\Characters\Character1\3D\Common\Mt_outline_hair.mat"]:
    t=open(f,encoding='utf-8',errors='replace').read()
    m=re.search(r'm_Shader: \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: 3\}',t)
    print("  m_Shader", m.groups())
# shader guid lookup
for meta in glob.glob(root+r"\Shaders\*.meta"):
    t=open(meta,encoding='utf-8',errors='replace').read(400)
    if 'c3d34d8d15ae50345aaa9746f98e23a2' in t:
        print("  shader guid c3d34d8d... ->", meta[:-5].replace(root+os.sep,""))
