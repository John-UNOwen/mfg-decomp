import re, os
OUT = r"C:\mfg\build\_tmp_audit"
got = open(os.path.join(OUT, "shader_blob.bin"), "rb").read()
# print printable strings with offsets
strs = []
for m in re.finditer(rb"[\x20-\x7e]{4,}", got):
    strs.append((m.start(), m.group().decode("latin-1")))
print("total strings:", len(strs))
# strings in the first 20k (param name tables)
for o, s in strs:
    if o < 20000:
        print(f"{o:7d}  {s}")
