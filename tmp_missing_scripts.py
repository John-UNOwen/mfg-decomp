# Find MonoBehaviours whose script object does not exist in each build,
# and report the owning GameObject + the script's owning file (dll/asm).
import UnityPy

def scan(env, tag):
    # identity of every loaded object (file, path_id)
    ids = set()
    for obj in env.objects:
        ids.add((obj.assets_file.name, obj.path_id))
    counts = {}
    for obj in env.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        try:
            data = obj.read_read_data if False else obj.read()
        except Exception:
            data = None
        try:
            raw = obj.read()
        except Exception:
            continue
        try:
            ptr = raw.m_Script
        except Exception:
            continue
        tgt_file = getattr(ptr, "assets_file", None)
        key = None
        if tgt_file is not None:
            key = (tgt_file.name, ptr.path_id)
        if key is not None and key in ids:
            continue
        # unresolved -> collect owner name
        owner = ""
        try:
            goptr = raw.m_GameObject
            go = goptr.read()
            owner = getattr(go, "m_Name", "") or ""
        except Exception:
            pass
        fname = tgt_file.name if tgt_file is not None else "?"
        key2 = (fname, owner)
        counts[key2] = counts.get(key2, 0) + 1
    print(f"== {tag}: unresolved MonoBehaviour scripts")
    for (fname, owner), n in sorted(counts.items(), key=lambda kv: -kv[1]):
        print(f"   scriptfile={fname} owner={owner!r} count={n}")

def main():
    env_w = UnityPy.load(r"C:\mfg\build\unity-project\out\webgl\Build\webgl.data")
    scan(env_w, "WEBGL")
    env_d = UnityPy.load(r"C:\mfg\build\unity-project\out\standalone\MFG_Data")
    scan(env_d, "DESKTOP")

if __name__ == "__main__":
    main()
