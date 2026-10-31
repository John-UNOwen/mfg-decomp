import json, base64, os, re
import lz4.block
OUT = r"C:\mfg\build\_tmp_audit"
j = json.load(open(r"C:\mfg\build\export-primary-content\Assets\Shaders\Character\Eye.json", encoding="utf-8"))
raw = base64.b64decode(j["m_CompressedBlob"])
target = j["m_DecompressedLengths"][0][0]
got = None
for skip in (0, 2, 4, 6, 8, 12):
    try:
        got = lz4.block.decompress(raw[skip:], uncompressed_size=target)
        print("lz4 ok skip", skip, "len", len(got)); break
    except Exception as e:
        print("skip", skip, "->", str(e)[:80])
assert got
fn = os.path.join(OUT, "shader_blob.bin")
open(fn, "wb").write(got)
txt = got.decode("latin-1")
for s in ("_EyeTex", "_EyeMaskTex", "_PupilTex", "_EyeBrowTex", "_EyeBrowMaskTex", "_StencilMask", "_StencilEyeMask", "_EyeOffsetX", "_HeightTex", "texture2D", "gl_FragColor", "!!", "_ClippingMask", "_MaterialMaskTex", "TEXCOORD", "texGen", "COMBINE", "GL_"):
    idx = [m.start() for m in re.finditer(re.escape(s), txt)]
    print(f"{s!r}: {len(idx)} first {idx[:6]}")
# program text sections
i = txt.find("!!")
if i >= 0:
    seg = txt[i:i+800]
    print("first '!!' section:", seg.replace("\x00", " ")[:600])
