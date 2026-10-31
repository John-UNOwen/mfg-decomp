# Full-build scan: enumerate Shader objects + tile Materials in the desktop
# build (all MFG_Data files) and the WebGL build, and probe path ids.
import os, glob
import UnityPy

DESKTOP_DIR = r"C:\mfg\build\unity-project\out\standalone\MFG_Data"
WEBGL_DATA = r"C:\mfg\build\unity-project\out\webgl\Build\webgl.data"
TILE_MATS = ("Hai", "Cube", "HaiSkin2", "HaiSe2", "Hai_Debug", "KiraRed")
WANT = {10760, 46}

def report(env, tag):
    shaders = []
    for obj in env.objects:
        if obj.type.name == "Shader":
            try:
                name = obj.read().m_Name
            except Exception:
                name = "<unreadable>"
            shaders.append((obj.assets_file.name, obj.path_id, name))
    print(f"== {tag}: {len(shaders)} shaders")
    seen = {}
    for fn, pid, name in shaders:
        seen.setdefault(pid, []).append((fn, name))
    for pid in sorted(seen):
        if pid in WANT or pid > 10000:
            print(f"   pid={pid} files={sorted(set(f for f,_ in seen[pid]))} names={sorted(set(n for _,n in seen[pid]))[:3]}")
    for obj in env.objects:
        if obj.type.name != "Material":
            continue
        try:
            m = obj.read()
        except Exception:
            continue
        if (getattr(m, "m_Name", "") or "") in TILE_MATS:
            sh = m.m_Shader
            try:
                target = sh.read()
                sname = repr(target.m_Name)
                sfile = target.assets_file.name
                spid = sh.path_id
            except Exception:
                sname = "UNRESOLVED/MISSING"
                sfile = str(getattr(sh, "assets_file", "?"))
                spid = getattr(sh, "path_id", "?")
            print(f"   MAT {m.m_Name!r} in {obj.assets_file.name} -> shader pid={spid} file={sfile} name={sname}")

def main():
    print("## DESKTOP build")
    for path in sorted(glob.glob(os.path.join(DESKTOP_DIR, "*"))):
        if path.endswith((".assets", ".resS")) or os.path.basename(path) == "globalgamemanagers":
            base = os.path.basename(path)
            try:
                env = UnityPy.load(path)
                report(env, f"desktop {base}")
            except Exception as e:
                print(f"desktop {base}: load fail {e}")
    print("## WEBGL build")
    env = UnityPy.load(WEBGL_DATA)
    print("sub-files:", sorted({getattr(f, "name", "?") for f in env.files}))
    report(env, "webgl")

if __name__ == "__main__":
    main()
