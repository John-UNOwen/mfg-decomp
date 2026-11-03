import re, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
import pathlib
p = pathlib.Path(r"C:\mfg\build\unity-project\Assets\Assembly-CSharp\MFG.Types.PlayerItem")
for f in p.glob("*.cs"):
    print("FILE", f.name)
src = [f for f in p.glob("*.cs") if "enum ItemID" in f.read_text(encoding="utf-8", errors="replace")]
f = src[0]
body = re.search(r"public enum ItemID\s*\{(.*?)\n\}", f.read_text(encoding="utf-8", errors="replace"), re.S).group(1)
body = re.sub(r"//.*", "", body)
val = -1
for tok in body.split(","):
    tok = tok.strip()
    if not tok:
        continue
    m = re.match(r"^(\w+)(?:\s*=\s*(-?\d+))?$", tok)
    if not m:
        continue
    val = int(m.group(2)) if m.group(2) else val + 1
    if val <= 14:
        print(val, m.group(1))
