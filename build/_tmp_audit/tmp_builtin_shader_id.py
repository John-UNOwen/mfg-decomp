# Resolve Unity built-in shader fileIDs to names using the editor's
# "unity default resources" SerializedFile (read-only inspection).
import sys
import UnityPy

path = r"C:\Program Files\Unity 2022.3.13f1\Editor\Data\Resources\unity default resources"
wanted = {10760, 10755, 10754, 10758, 10770, 10720, 46, 4800000, 10753}

env = UnityPy.load(path)
for obj in env.objects:
    if obj.type.name != "Shader":
        continue
    if obj.path_id in wanted:
        data = obj.read()
        print(obj.path_id, "=", getattr(data, "m_Name", "?"))

# also dump all built-in shader names for the record
names = []
for obj in env.objects:
    if obj.type.name == "Shader":
        try:
            names.append((obj.path_id, obj.read().m_Name))
        except Exception:
            pass
print("total shaders:", len(names))
for pid, n in sorted(names):
    if pid in (10760, 10755, 10753, 10754, 10758, 10770, 10720, 46):
        print("CONFIRM", pid, n)
