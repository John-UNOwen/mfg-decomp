# Compare built-in shader inclusion + tile material resolution between the
# desktop build and the WebGL build of the MFG rebuild.
import UnityPy

DESKTOP = r"C:\mfg\build\unity-project\out\standalone\MFG_Data"
WEBGL_DATA = r"C:\mfg\build\unity-project\out\webgl\Build\webgl.data"

WANT_IDS = {10760, 10755, 10753, 10754, 10758, 10770, 10720, 46, 7, 10703, 10704, 10701}

def scan_shaders(env, tag):
    print(f"== {tag}: Shader objects ==")
    for obj in env.objects:
        if obj.type.name == "Shader":
            try:
                name = obj.read().m_Name
            except Exception:
                name = "<unreadable>"
            print(f"   file={obj.assets_file.name} path_id={obj.path_id} name={name!r}")

def scan_mats(env, tag):
    for obj in env.objects:
        if obj.type.name != "Material":
            continue
        try:
            m = obj.read()
        except Exception:
            continue
        nm = getattr(m, "m_Name", "") or ""
        if nm in ("Hai", "Cube", "HaiSkin2", "HaiSe2", "Hai_Debug", "KiraRed", "HaiSkin"):
            sh = None
            try:
                sh = m.m_Shader  # PPtr
                info = f"shader path_id={sh.path_id} file={getattr(sh.assets_file,'name','?')} resolved={sh.peek() is not None and sh.read().m_Name if sh.peek() else 'MISSING'}"
            except Exception as e:
                info = f"shader ptr error {e}"
            tex = None
            try:
                for te in (m.m_SavedProperties.m_TexEnvs if hasattr(m, "m_SavedProperties") else []):
                    pass
            except Exception:
                pass
            print(f"== {tag} Material {nm!r} file={obj.assets_file.name} {info}")

def main():
    files = [
        (f"{DESKTOP}\\globalgamemanagers", "desktop ggmgr"),
        (f"{DESKTOP}\\globalgamemanagers.assets", "desktop ggmgr.assets"),
        (WEBGL_DATA, "webgl data"),
    ]
    for path, tag in files:
        try:
            env = UnityPy.load(path)
        except Exception as e:
            print(f"== {tag}: load failed: {e}")
            continue
        scan_shaders(env, tag)
        scan_mats(env, tag)

    # also scan the built-in resources shipped inside the WebGL build file list
    import os
    print("== webgl data sub-files ==")
    env = UnityPy.load(WEBGL_DATA)
    for f in env.files:
        print("  ", getattr(f, "name", "?"))

if __name__ == "__main__":
    main()
