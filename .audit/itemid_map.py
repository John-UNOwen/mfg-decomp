import re, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
src = open(r"C:\mfg\build\unity-project\Assets\Assembly-CSharp\MFG.Types.Customize.CutinItem\ItemID.cs", encoding="utf-8").read()
m = re.search(r"public enum ItemID\s*\{(.*?)\n\}", src, re.S)
body = m.group(1)
body = re.sub(r"//.*", "", body)
val = -1
assign = {}
for tok in body.split(","):
    tok = tok.strip()
    if not tok:
        continue
    mm = re.match(r"^(\w+)(?:\s*=\s*(-?\d+))?$", tok)
    if not mm:
        print("SKIP token:", tok); continue
    name, num = mm.group(1), mm.group(2)
    val = int(num) if num is not None else val + 1
    assign[name] = val
print("first:", list(assign.items())[:4])
for i in range(12, 22):
    name = f"Item{i:02d}AgariSR01"
    print(name, "=", assign.get(name))
# CharaType ordinals
src2 = open(r"C:\mfg\build\unity-project\Assets\Assembly-CSharp\MFG.Types\CharaType.cs", encoding="utf-8").read()
m2 = re.search(r"public enum CharaType\s*\{(.*?)\n\}", src2, re.S)
print("CharaType enum head:", m2.group(1).strip()[:300] if m2 else "NOT FOUND")
