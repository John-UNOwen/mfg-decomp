import UnityPy

BD = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64\3633e69d040cf55c4855557f2daec047.bundle"
env = UnityPy.load(BD)
print("loadable:", list(env.loadable_files))
for k, f in env.loadable_files.items():
    print(k, type(f).__name__, getattr(f, "size", ""))
    inner = getattr(f, "files", None)
    if inner:
        print("   inner:", list(inner))
