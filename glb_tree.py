import json
g = json.load(open(r"C:\mfg\glb_parsed.json"))
nodes = g["nodes"]
def dump(i, depth=0):
    n = nodes[i]
    extra = ""
    if "mesh" in n: extra += f" mesh={n['mesh']}"
    if "skin" in n: extra += f" skin={n['skin']}"
    if "extras" in n: extra += f" extras={json.dumps(n['extras'])}"
    print("  "*depth + f"[{i}] {n.get('name','?')}{extra}")
    for c in n.get("children",[]):
        dump(c, depth+1)
root = g["scenes"][0]["nodes"][0]
print("ROOT", root, nodes[root].get("name"))
# print whole tree but compress: print children count
def count(i):
    return 1+sum(count(c) for c in nodes[i].get("children",[]))
dump(root)
