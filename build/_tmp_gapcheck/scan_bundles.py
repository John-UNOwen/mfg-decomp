import os, sys, time

BD = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64"
NEEDLE = "CharaSelectNum1".encode("utf-8")

files = sorted(f for f in os.listdir(BD) if f.endswith(".bundle"))
print("files:", len(files))
hits = []
t0 = time.time()
CH = 300000
for n, f in enumerate(files, 1):
    p = os.path.join(BD, f)
    with open(p, "rb") as fh:
        head = fh.read(CH)
        if NEEDLE in head:
            hits.append(f)
            print("HIT", f, flush=True)
        elif len(head) == CH:
            # extend read for this file in case name table sits further out
            rest = fh.read(2_000_000)
            if NEEDLE in rest:
                hits.append(f)
                print("HIT-deep", f, flush=True)
    if n % 4000 == 0:
        print(f"  {n}/{len(files)} t={time.time()-t0:.0f}s", flush=True)
print("done hits:", hits)
