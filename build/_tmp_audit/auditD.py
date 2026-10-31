import json, zlib, base64, re, binascii, os
OUT = r"C:\mfg\build\_tmp_audit"
j = json.load(open(r"C:\mfg\build\export-primary-content\Assets\Shaders\Character\Eye.json", encoding="utf-8"))
ss = j["m_ParsedForm"]["m_SubShaders"][0]
for i, p in enumerate(ss["m_Passes"]):
    print(f"PASS {i}: type={p['m_Type']} name={p['m_Name']!r} tags={p['m_Tags']} texName={p['m_TextureName']!r}")
    print("  m_State:", json.dumps(p["m_State"]))
    print("  progMask:", p["m_ProgramMask"], "progVert:", p["m_ProgVertex"], "progFrag:", p["m_ProgFragment"], "domain:", json.dumps(p["m_ProgDomain"]), "platforms:", json.dumps(p["m_Platforms"])[:200])
print("Offsets:", j["m_Offsets"], "StageCounts:", j["m_StageCounts"], "DecompressedLengths:", j["m_DecompressedLengths"], "CompressedLengths:", j["m_CompressedLengths"])
blob = j["m_CompressedBlob"]
raw = base64.b64decode(blob) if isinstance(blob, str) else bytes(blob)
print("blob bytes:", len(raw), raw[:8].hex())
try:
    dz = zlib.decompress(raw)
except Exception:
    try: dz = zlib.decompressobj(-15).decompress(raw)
    except Exception as e:
        dz = b""
        print("inflate fail:", e)
print("inflated:", len(dz))
open(os.path.join(OUT, "blob.txt"), "wb").write(dz)
# find GLSL text markers
for marker in (b"!!", b"GLSL", b"ESSL", b"#version", b"gl_Position", b"gl_FragColor"):
    idxs = [m.start() for m in re.finditer(re.escape(marker), dz)][:5]
    print(marker, idxs)
