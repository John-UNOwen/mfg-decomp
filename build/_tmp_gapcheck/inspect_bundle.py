import UnityPy

BD = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64"
for b in ("3633e69d040cf55c4855557f2daec047.bundle",):
    env = UnityPy.load(BD + "\\" + b)
    print("==", b)
    for name, f in env.files.items():
        print("  file:", name, "type:", getattr(f, "type", None), "size:", len(getattr(f, "full_data", b"")))
    for obj in env.objects:
        print("  obj:", getattr(obj.type, "name", "?"), obj.path_id, getattr(obj, "byte_size", ""))
