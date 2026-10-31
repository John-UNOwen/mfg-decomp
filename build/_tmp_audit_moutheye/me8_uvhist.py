import re, struct, sys

def uv_hist(path):
    t = open(path, encoding="utf-8", errors="ignore").read()
    n = int(re.search(r"m_VertexCount: (\d+)", t).group(1))
    ch = re.findall(r"- stream: (\d+)\n\s+offset: (\d+)\n\s+format: (\d+)\n\s+dimension: (\d+)", t)
    blob = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", t).group(1))
    mx = {}
    for s, off, fmt, dim in ch:
        s, off, dim, fmt = int(s), int(off), int(dim), int(fmt)
        if dim == 0:
            continue
        esz = 1 if fmt in (2, 10) else 4
        mx[s] = max(mx.get(s, 0), off + dim * esz)
    base, cur = {}, 0
    for s in sorted(mx):
        base[s] = cur
        cur += mx[s] * n
    out = {}
    for s, off, fmt, dim in ch:
        s, off, fmt, dim = int(s), int(off), int(fmt), int(dim)
        if dim != 2 or fmt != 0:
            continue
        vals = []
        stride = mx[s]
        for i in range(n):
            v = struct.unpack_from("<2f", blob, base[s] + i * stride + off)
            vals.append(v)
        # y histogram in 8 bins [0,0.125)... plus x histogram
        hy = [0] * 8
        hx = [0] * 8
        for u, v in vals:
            by = min(7, max(0, int(v * 8)))
            bx = min(7, max(0, int(u * 8)))
            hy[by] += 1
            hx[bx] += 1
        out[f"stream{s}@{off}"] = (hx, hy)
    return n, out

for name in sys.argv[1:]:
    import os
    n, o = uv_hist(name)
    print("==", os.path.basename(name), "verts:", n)
    for k, (hx, hy) in sorted(o.items()):
        print("  ", k, "x-hist[0..7]=", hx)
        print("  ", k, "y-hist[0..7]=", hy)
