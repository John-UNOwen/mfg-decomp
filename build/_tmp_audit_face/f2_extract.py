import json, base64, lz4.block, os, re
OUT = r"C:\mfg\build\_tmp_audit_face"
p = r"C:\mfg\build\export-primary-content\Assets\Toon\Shader\Toon_DoubleShadeWithFeather_TransClipping_StencilMask.json"
j = json.load(open(p, encoding="utf-8"))
raw = base64.b64decode(j["m_CompressedBlob"])
target = j["m_DecompressedLengths"][0][0]
print("raw", len(raw), "target", target, "magic", raw[:4].hex())
got = None
for skip in (0,2,4,6,8,12):
    try:
        got = lz4.block.decompress(raw[skip:], uncompressed_size=target)
        print("lz4 ok skip", skip, "len", len(got)); break
    except Exception as e:
        print("skip", skip, "->", str(e)[:70])
assert got
fn = os.path.join(OUT, "face_blob.bin")
open(fn,"wb").write(got)
print("wrote", fn)
print("head:", got[:64])
# DXBC programs
offs = [m.start() for m in re.finditer(rb"DXBC", got)]
print("DXBC magic offsets:", offs)
# pass-name-ish strings near top
txt = got.decode("latin-1")
for s in ("FORWARD","FORWARD_DELTA","ShadowCaster","Shadowcaster","Meta","FACE","Face"," silhouette","Silhouette","StencilMask","UNLIT","BaseRemap","DepthNormals","ScenePickingMask"):
    idx = [m.start() for m in re.finditer(re.escape(s), txt)]
    print(f"{s!r}: {len(idx)} {idx[:8]}")
