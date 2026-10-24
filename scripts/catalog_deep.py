"""Deeper analysis of the shipped Addressables catalog internal ids.

Reports: shape of m_InternalIds (bundle entries vs bundled-asset entries vs scenes),
resource type list, and sample asset paths for AssetRipper export cross-checking.
Writes json to the path given as third argument (default build/catalog-stats.json).
"""
import json
import os
import sys
from collections import Counter

INSTALL_CATALOG = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"

PREFIX = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}\\StandaloneWindows64\\"


def main():
    path = sys.argv[1] if len(sys.argv) > 1 else INSTALL_CATALOG
    out_path = sys.argv[2] if len(sys.argv) > 2 else r"build\catalog-stats.json"
    with open(path, "r", encoding="utf-8") as f:
        cat = json.load(f)
    ids = cat["m_InternalIds"]
    kinds = Counter()
    bundle_entries = []
    asset_entries = []
    scene_entries = []
    for i in ids:
        if not isinstance(i, str):
            kinds["nonstring"] += 1
            continue
        s = i[len(PREFIX):] if i.startswith(PREFIX) else i
        if "/" not in s:
            kinds["bundle_only"] += 1
            bundle_entries.append(s)
        elif s.lower().endswith(".unity") or ".unity/" in s.lower():
            kinds["scene"] += 1
            scene_entries.append(s)
        else:
            kinds["bundled_asset"] += 1
            asset_entries.append(s)
    types = [(t.get("m_AssemblyName", "").split(",")[0], t.get("m_ClassName", "")) for t in cat["m_resourceTypes"]]
    top_paths = Counter(e.split("/")[-2] if "/" in e else "" for e in asset_entries).most_common(15)
    report = {
        "catalog": path,
        "internal_ids": len(ids),
        "kinds": dict(kinds),
        "resource_types": ["%s.%s" % t for t in types],
        "asset_path_parent_dirs": dict(top_paths),
        "sample_bundles": bundle_entries[:5],
        "sample_assets": asset_entries[:15],
        "sample_scenes": scene_entries[:15],
        "prefixes_stripped": PREFIX,
        "provider_ids": cat["m_ProviderIds"],
        "locator_id": cat["m_LocatorId"],
        "build_result_hash": cat["m_BuildResultHash"],
    }
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2, ensure_ascii=False)
    print(json.dumps(report, indent=2, ensure_ascii=False)[:4000])


if __name__ == "__main__":
    main()
