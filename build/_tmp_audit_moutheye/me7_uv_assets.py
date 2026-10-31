import re, struct, sys

def uv_ranges(path):
    t = open(path, encoding="utf-8", errors="ignore").read()
    m = re.search(r"m_VertexCount: (\d+)", t)
    n = int(m.group(1))
    # channel table: capture stream/offset/format/dimension list
    ch = re.findall(r"- stream: (\d+)\n\s+offset: (\d+)\n\s+format: (\d+)\n\s+dimension: (\d+)", t)
    blob_m = re.search(r"_typelessdata: ([0-9a-fA-F]+)", t)
    blob = bytes.fromhex(blob_m.group(1))
    # compute stream sizes
    per_stream = {}
    mx = {}
    for s, off, fmt, dim in ch:
        s, off, dim = int(s), int(off), int(dim)
        if dim == 0:
            continue
        stride_end = off + dim * 4  # float32 formats (format 0/10..); format10 byte handled below
        if int(fmt) in (2, 10):  # byte normalized / byte: 1 byte per comp
            stride_end = off + dim * 1
        mx[s] = max(mx.get(s, 0), stride_end)
    for s in mx:
        per_stream[s] = n * mx[s]
    # stream layout in blob: stream0 then 1 then 2 (order of appearance), each padded? total = m_DataSize
    total_m = re.search(r"m_DataSize: (\d+)", t)
    total = int(total_m.group(1))
    order = sorted(per_stream)
    base = {}
    cur = 0
    for s in order:
        base[s] = cur
        cur += per_stream[s]
    # if total has slack, ignore (we only need uv streams)
    out = {}
    for s, off, fmt, dim in ch:
        s, off, fmt, dim = int(s), int(off), int(fmt), int(dim)
        if dim != 2 or int(fmt) != 0:
            continue
        key = f"uv{len(out)}" if False else f"stream{s}@{off}"
        vals = [[1e9, 1e9], [-1e9, -1e9]]
        stride = mx[s]
        for i in range(n):
            v = struct.unpack_from("<2f", blob, base[s] + i * stride + off)
            for c in (0, 1):
                vals[0][c] = min(vals[0][c], v[c])
                vals[1][c] = max(vals[1][c], v[c])
        out[key] = (round(vals[0][0], 4), round(vals[0][1], 4), round(vals[1][0], 4), round(vals[1][1], 4))
    return n, out, base, mx

for name in sys.argv[1:]:
    n, o, base, mxs = uv_ranges(name)
    import os
    print(os.path.basename(name), "verts:", n, "streams:", base, mxs)
    for k in sorted(o):
        print("   ", k, o[k])
