import json, os, struct
import lz4.block

BUNDLE = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64\3633e69d040cf55c4855557f2daec047.bundle"
OUT = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\unity-project-missing\Assets\MFG\Fonts")
import UnityPy

# --- 1. UnityFS container name table (proof of asset path) ---
raw = open(BUNDLE, "rb").read()
i = raw.find(b"CharaSelectNum1")
if i >= 0:
    print("name-table occurrence at", i, ":",
          raw[max(0, i - 40):i + 45])
assert raw[:7] == b"UnityFS"
p = 8
ver, = struct.unpack_from("<I", raw, p); p += 4
e = raw.index(b"\x00", p); p = e + 1  # builder string
file_size, ci_size, ui_size = struct.unpack_from("<3Q", raw, p); p += 24
flags, = struct.unpack_from("<I", raw, p); p += 4
comp = flags & 0x3F
ci = raw[p:p + ci_size]
if comp == 3:
    bi = lz4.block.decompress(ci, uncompressed_size=ui_size)
elif comp == 0:
    bi = ci
else:
    raise SystemExit(f"unsupported compression {comp}")
p += ci_size
p = (p + 15) & ~15  # 16-byte align
q = 0
if ver >= 7:
    q += 8  # raw data size u64
blocks_count, = struct.unpack_from("<I", bi, q); q += 4
if ver >= 7:
    q += 8  # blocks data size? + flags? probe
    # SerializedFile blocksInfo v7 layout: rawSize u64, blocksCount u32, blocksDataSize? then flags u32
    # UnityPy layout: [u64 uncompressedDataSize][u32 blocksCount][u32 blocksDataSize? no]...
    # To be safe: parse blocks entries as {u32 unc, u32 flags, u32? } until names string appears
# Fallback: find file-name strings directly: names table = printable strings after blocks info
names = []
import re
for m in re.finditer(rb"[\x20-\x7e]{10,}\x00", bi):
    names.append(m.group()[:-1].decode("latin-1"))
print("ver", ver, "flags", hex(flags), "blocks", blocks_count if ver >= 7 else None)
print("strings inside blocksInfo:", names[:10])
# names table also follows blocks info in the *bundle* raw stream; search file region
tail_start = p
region = raw[tail_start:tail_start + 4096]
fnames = [m.group()[:-1].decode("latin-1") for m in re.finditer(rb"[\x20-\x7e]{10,}\x00", region)]
print("file-name table region strings:", fnames[:8])

# --- 2. extract the Material object ---
env = UnityPy.load(BUNDLE)
obj = None
node = None
for o in env.objects:
    if getattr(o.type, "name", "?") != "Material":
        continue
    obj = o
    try:
        node = o.read()
    except Exception:
        pass
if obj is None:
    raise SystemExit("no Material in bundle")
os.makedirs(OUT, exist_ok=True)
stem = "DFG-PopMix-W5 CharaSelectNum1.mat"

# typetree dump attempt (UnityPy 1.25 helpers)
row = {"asset_path": "Assets/MFG/Fonts/DFG-PopMix-W5 CharaSelectNum1.mat",
       "bundle": os.path.basename(BUNDLE), "path_id": int(obj.path_id), "type": "Material",
       "name_in_bundle": str(getattr(node, "m_Name", "") if node else "")}
try:
    from UnityPy.helpers import TypeTreeHelper
    done = False
    for fname in ("DumpAttribute", "DumpValues"):
        fn = getattr(TypeTreeHelper, fname, None)
        if fn is None:
            continue
        try:
            json.dump(fn(node if node else obj), open(os.path.join(OUT, stem + ".json"), "w", encoding="utf-8"),
                      indent=1, ensure_ascii=False, default=str)
            done = True
            print("typetree json via", fname)
            break
        except Exception as exc:
            row[fname + "_error"] = repr(exc)[:200]
    row["json"] = stem + ".json" if done else None
except Exception as exc:
    row["helpers_error"] = repr(exc)[:200]

def attrs_dump(node):
    # Saved properties only: walking the full UnityPy object tree follows the
    # assets_file<->assetbundle backrefs and explodes into hundreds of MB of JSON.
    sp = getattr(node, "m_SavedProperties", None)
    out = {"name_in_bundle": getattr(node, "m_Name", None),
           "shader": str(getattr(node, "m_Shader", None))}
    props = {}
    if sp is not None:
        for attr, key in (("m_Floats", "floats"), ("m_Colors", "colors"), ("m_TexEnvs", "texenvs")):
            v = getattr(sp, attr, None)
            props[key] = [list(x) if not isinstance(x, dict) else x for x in (v or [])]
    out["properties"] = props
    return out

try:
    if node is not None:
        json.dump(attrs_dump(node), open(os.path.join(OUT, stem + ".attrs.json"), "w", encoding="utf-8"),
                  indent=1, ensure_ascii=False, default=str)
        print("attrs json ok")
except Exception as exc:
    row["attrs_error"] = repr(exc)[:300]

b = obj.get_raw_data()
open(os.path.join(OUT, stem + ".bin"), "wb").write(b)
row["bin_bytes"] = len(b)
os.makedirs(os.path.dirname(os.path.dirname(OUT)), exist_ok=True)
mani = {"note": "Targeted UnityPy extraction; catalog resolver (extract_missing_unitypy.py) cannot map this entry: "
                "this build's catalog m_InternalIds are separate bundle-file ids and bare asset paths (no '.bundle/<path>' concatenation).",
        "rows": [row]}
json.dump(mani, open(os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\unity-project-missing\manifest.json"), "w", encoding="utf-8"),
          indent=1, ensure_ascii=False)
print(json.dumps(row, indent=1))
