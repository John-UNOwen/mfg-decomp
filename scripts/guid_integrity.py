"""Verify GUID integrity of the AssetRipper project export.

1. Index every .meta in the exported project: guid -> asset path (and assetBundleName when set).
2. Scan the 62 exported scenes and every .prefab for m_Script guid references and report how
   many resolve to a guid in the index (script mapping integrity).
3. Cross-check one catalog entry: bundle hash in aa/catalog.json vs the assetBundleName that
   AssetRipper wrote into that asset's .meta.

Usage: python scripts/guid_integrity.py [exported_project_root]
"""
import json
import os
import re
import sys
from collections import Counter

INSTALL_CATALOG = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"
GUID_RE = re.compile(r"guid: ([0-9a-f]{32})")
SCRIPT_RE = re.compile(r"fileID: 11500000,\s*guid: ([0-9a-f]{32})", re.S)

ABNAME_RE = re.compile(r"assetBundleName: ([0-9a-f]{16,})")


def build_index(root):
    index = {}
    dupes = 0
    bundle_names = Counter()
    for dirpath, _dirs, files in os.walk(root):
        for fn in files:
            if not fn.endswith(".meta"):
                continue
            path = os.path.join(dirpath, fn)
            try:
                with open(path, "r", encoding="utf-8", errors="replace") as f:
                    text = f.read()
            except OSError:
                continue
            g = GUID_RE.search(text)
            if not g:
                continue
            guid = g.group(1)
            if guid in index:
                dupes += 1
            index[guid] = os.path.relpath(path[:-5], root).replace("\\", "/")
            m = ABNAME_RE.search(text)
            if m:
                bundle_names[m.group(1)] += 1
    return index, dupes, bundle_names


def main():
    root = sys.argv[1] if len(sys.argv) > 1 else r"build\unity-project"
    bundles_dir = sys.argv[2] if len(sys.argv) > 2 else r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64"
    assets = os.path.join(root, "Assets")
    index, dupes, bundle_names = build_index(assets)
    print(f"meta_guids={len(index)} duplicate_guids={dupes} metas_with_bundle_name={sum(bundle_names.values())} distinct_bundle_names={len(bundle_names)}")

    refs = Counter()
    files_with_refs = 0
    for dirpath, _dirs, files in os.walk(assets):
        for fn in files:
            if not fn.endswith((".unity", ".prefab", ".asset")):
                continue
            path = os.path.join(dirpath, fn)
            try:
                with open(path, "r", encoding="utf-8", errors="replace") as f:
                    text = f.read()
            except OSError:
                continue
            found = SCRIPT_RE.findall(text)
            if found:
                files_with_refs += 1
            for g in found:
                refs[g] += 1
    total = sum(refs.values())
    resolved = sum(c for g, c in refs.items() if g in index)
    print(f"yaml_files_with_script_refs={files_with_refs} script_refs={total} distinct_script_guids={len(refs)} resolved={resolved} unresolved={total - resolved}")

    # every guid reference in every yaml text file (textures, meshes, materials, controllers, ...)
    any_ref = re.compile(r"guid: ([0-9a-f]{32})")
    all_total = 0
    all_resolved = 0
    unresolved_samples = Counter()
    for dirpath, _dirs, files in os.walk(assets):
        for fn in files:
            if not fn.endswith((".unity", ".prefab", ".asset", ".mat", ".anim", ".controller",
                                ".overrideController", ".playable", ".lighting", ".shader", ".rendertexture")):
                continue
            path = os.path.join(dirpath, fn)
            try:
                with open(path, "r", encoding="utf-8", errors="replace") as f:
                    text = f.read()
            except OSError:
                continue
            for g in any_ref.findall(text):
                all_total += 1
                if g in index:
                    all_resolved += 1
                else:
                    unresolved_samples[os.path.relpath(path, assets).replace("\\", "/")] += 1
    if all_total:
        pct = round(100.0 * all_resolved / all_total, 3)
        print(f"all_asset_references={all_total} resolved={all_resolved} ({pct}%) unresolved={all_total - all_resolved}")
        print("files with the most unresolved references:")
        for p, c in unresolved_samples.most_common(8):
            print(f"  {c:6}  {p}")

    unresolved_top = [(g, c, index.get(g)) for g, c in refs.most_common(200) if g not in index][:10]
    for g, c, _ in unresolved_top:
        print("  unresolved guid", g, "refs", c)

    # catalog bundle hash vs meta assetBundleName
    target = "DFG-HeiSeiGothic-W7 Bingo.mat"
    with open(INSTALL_CATALOG, "r", encoding="utf-8") as f:
        cat = json.load(f)
    cid = next((i for i in cat["m_InternalIds"] if isinstance(i, str) and i.replace("\\", "/").endswith(target)), None)
    print("catalog id for", target, "->", cid)
    meta_path = os.path.join(assets, target.replace("/", os.sep) + ".meta")
    if os.path.exists(meta_path):
        with open(meta_path, "r", encoding="utf-8", errors="replace") as f:
            text = f.read()
        print("meta assetBundleName match:", (ABNAME_RE.search(text) or ["", ""])[1], "guid:", (GUID_RE.search(text) or ["", ""])[1])
    print("sample of bundle names recorded in metas:", list(bundle_names)[:5])

    # do the assetBundleName values match the shipped bundle file names?
    try:
        shipped = {fn[:-len(".bundle")].lower() for fn in os.listdir(bundles_dir) if fn.endswith(".bundle")}
    except OSError as exc:
        shipped = set()
        print("could not list bundle dir:", repr(exc)[:120])
    named = {k.lower() for k in bundle_names}
    both = sum(bundle_names[k] for k in bundle_names if k.lower() in shipped)
    print(f"shipped_bundle_files={len(shipped)} metas_naming_a_shipped_bundle={both} of {sum(bundle_names.values())}")
    print("meta bundle names that are shipped bundle files:", len(named & shipped))


if __name__ == "__main__":
    main()
