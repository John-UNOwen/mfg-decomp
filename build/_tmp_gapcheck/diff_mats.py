import re, difflib, os

MD = r"C:\mfg\build\unity-project\Assets\Material"
def props(p):
    t = open(p, encoding="utf-8").read()
    i = t.find("m_SavedProperties:")
    return t[i:]

a = props(MD + r"\DFG-PopMix-W5 1.mat")
b = props(MD + r"\DFG-PopMix-W5 Black.mat")
diff = list(difflib.unified_diff(a.splitlines(), b.splitlines(), lineterm="", n=0))
print("diff lines (1.mat vs Black.mat):", len(diff))
for d in diff[:60]:
    print(d)
print("=== meta of 1.mat:")
print(open(MD + r"\DFG-PopMix-W5 1.mat.meta", encoding="utf-8").read())
files = sorted(os.listdir(MD))
plain = [f for f in files if f.startswith("DFG-PopMix-W5") and " 1" in f]
print("files with ' 1':", plain)
print("has plain W5.mat:", "DFG-PopMix-W5.mat" in files)
