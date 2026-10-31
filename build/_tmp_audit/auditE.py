import json, base64, zlib, os, re
OUT = r"C:\mfg\build\_tmp_audit"
j = json.load(open(r"C:\mfg\build\export-primary-content\Assets\Shaders\Character\Eye.json", encoding="utf-8"))
raw = base64.b64decode(j["m_CompressedBlob"])
target = j["m_DecompressedLengths"][0][0]
print("raw", len(raw), "target", target)

def lz4_block(src, expected):
    d = bytearray(); i = 0; n = len(src)
    while i < n:
        token = src[i]; i += 1
        ll = token >> 4
        if ll == 15:
            while i < n and src[i] == 255: ll += 255; i += 1
            ll += src[i]; i += 1
        if i + ll > n: return None
        d += src[i:i+ll]; i += ll
        if i >= n: break
        if len(src) - i < 2: break
        off = int.from_bytes(src[i:i+2], "little"); i += 2
        if off == 0 or off > len(d): return None
        ml = token & 15
        if ml == 15:
            while i < n and src[i] == 255: ml += 255; i += 1
            ml += src[i]; i += 1
        ml += 3
        start = len(d) - off
        for k in range(ml): d.append(d[start+k])
        if len(d) >= expected: break
    if len(d) != expected: return None
    return bytes(d)

got = None
for skip in range(0, 24):
    r = lz4_block(raw[skip:], target)
    if r: got = r; print("lz4 ok at skip", skip); break
    # try raw deflate at skip
    try:
        r2 = zlib.decompressobj(-15).decompress(raw[skip:], target)
        if len(r2) > 50000: got = r2; print("deflate ok at skip", skip); break
    except Exception: pass
    try:
        r3 = zlib.decompress(raw[skip:])
        if len(r3) > 50000: got = r3; print("zlib ok at skip", skip); break
    except Exception: pass
if got is None:
    # LZ4 frame?
    print("frame magic:", raw[:4].hex(), "LZ4F magic 184d2204")
    r = lz4_block(raw[4:], target)
    print("lz4 at 4:", None if not r else len(r))
else:
    fn = os.path.join(OUT, "shader_blob.txt")
    open(fn, "wb").write(got)
    print("wrote", fn, len(got))
    txt = got.decode("utf-8", "ignore")
    for marker in ("!!", "GLSL", "ESSL", "#version", "gl_Position", "gl_FragColor", "texture2D", "TEXTURE_2D", "COMBINE"):
        idxs = [m.start() for m in re.finditer(re.escape(marker), txt)][:6]
        print(marker, idxs)
    print("first 300 chars:", txt[:300])
