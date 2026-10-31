import json, base64, zlib, os, re
OUT = r"C:\mfg\build\_tmp_audit"
j = json.load(open(r"C:\mfg\build\export-primary-content\Assets\Shaders\Character\Eye.json", encoding="utf-8"))
raw = base64.b64decode(j["m_CompressedBlob"])
target = j["m_DecompressedLengths"][0][0]

def lz4_block(src, expected):
    d = bytearray(); i = 0; n = len(src)
    while i < n and len(d) < expected:
        token = src[i]; i += 1
        ll = token >> 4
        if ll == 15:
            while i < n and src[i] == 255: ll += 255; i += 1
            if i >= n: break
            ll += src[i]; i += 1
        if i + ll > n: ll = n - i
        d += src[i:i+ll]; i += ll
        if len(d) >= expected or i >= n: break
        if i + 2 > n: break
        off = int.from_bytes(src[i:i+2], "little"); i += 2
        if off == 0 or off > len(d): break
        ml = token & 15
        if ml == 15:
            while i < n and src[i] == 255: ml += 255; i += 1
            if i >= n: break
            ml += src[i]; i += 1
        ml += 3
        start = len(d) - off
        for k in range(ml): d.append(d[start+k])
    return bytes(d[:expected]) if len(d) >= expected else None

res = {}
for skip in range(0, 32):
    r = lz4_block(raw[skip:], target)
    if r: res[("lz4", skip)] = r; break
    try:
        r2 = zlib.decompressobj(-15).decompress(raw[skip:])
        if len(r2) >= target: res[("deflate", skip)] = r2[:target]; break
    except Exception: pass
    try:
        r3 = zlib.decompress(raw[skip:])
        if len(r3) >= target: res[("zlib", skip)] = r3[:target]; break
    except Exception: pass
if res:
    (kind, skip), got = next(iter(res.items()))
    print("decoded via", kind, "skip", skip, "len", len(got))
    fn = os.path.join(OUT, "shader_blob.txt")
    open(fn, "wb").write(got)
    txt = got.decode("utf-8", "ignore")
    for marker in ("!!", "GLSL", "ESSL", "#version", "texture2D", "textureCube", "TEXTURE", "COMBINE", "gl_FragColor"):
        idxs = [m.start() for m in re.finditer(re.escape(marker), txt)][:8]
        print(f"  {marker!r}: {len(idxs)} shown {idxs}")
    print("head:", txt[:200].replace("\n", "\\n"))
else:
    print("all decode attempts failed")
    # header analysis
    print("first 64 bytes:", raw[:64].hex())
    print("entropy-ish: printable ratio", sum(32 <= b < 127 for b in raw[:2000])/2000)
