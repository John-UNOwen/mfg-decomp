import glob, os, re
HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
for f in glob.glob(HERE + r"\*_asm*.txt"):
    data = open(f, "rb").read()
    n = data.count(b"\x00")
    n11 = data.count(b"\x11")
    if n:
        # find where nulls start
        i = data.index(b"\x00")
        fixed = data.replace(b"\x00", b"")
        open(f, "wb").write(fixed)
        print(os.path.basename(f), "nulls:", n, "first at", i, "of", len(data), "x11:", n11)
