"""Coverage check: addressables catalog entries vs AssetRipper export tree.

For every catalog internal id of the form "<bundle>/<asset path>", compute a key = path
without its last extension (after stripping any ".json" wrapper AssetRipper adds), then
report how many keys exist in the export tree.

Usage: python scripts/catalog_coverage.py [catalog.json] [export_root] [out.json]
"""
import json
import os
import re
import sys
from collections import Counter

INSTALL_CATALOG = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"
PREFIX = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}\\StandaloneWindows64\\"
SUFFIX = re.compile(r"_[0-9]+$")


def strip_ext(p):
    return os.path.splitext(p)[0]


def main():
    catalog = sys.argv[1] if len(sys.argv) > 1 else INSTALL_CATALOG
    export_root = sys.argv[2] if len(sys.argv) > 2 else r"build\unity-project"
    out_path = sys.argv[3] if len(sys.argv) > 3 else r"build\catalog-coverage.json"

    with open(catalog, "r", encoding="utf-8") as f:
        cat = json.load(f)

    want = {}          # key -> catalog asset path
    bundle_ids = set()
    for raw in cat["m_InternalIds"]:
        if not isinstance(raw, str):
            continue
        s = raw[len(PREFIX):] if raw.startswith(PREFIX) else raw
        s = s.replace("\\", "/")
        if s.lower().endswith(".bundle"):
            bundle_ids.add(s.lower())
            continue
        # "<bundle>.bundle/<asset path>" -> asset path
        idx = s.find(".bundle/")
        apath = s[idx + len(".bundle/"):] if idx >= 0 else s
        key = strip_ext(apath).lower()
        if key:
            want[key] = apath

    have = set()
    have_norm = set()
    ext_counter = Counter()
    for root, _dirs, files in os.walk(os.path.join(export_root, "Assets")):
        for fn in files:
            rel = os.path.relpath(os.path.join(root, fn), export_root).replace("\\", "/")
            ext_counter[os.path.splitext(fn)[1].lower()] += 1
            body = rel[:-5] if rel.lower().endswith(".json") else rel   # drop AssetRipper's .json wrapper
            for key in (strip_ext(body).lower(), strip_ext(strip_ext(body)).lower()):
                have.add(key)
                have_norm.add(SUFFIX.sub("", key))                      # AssetRipper appends _N on name clashes

    hit = sum(1 for k in want if k in have)
    hit_norm = sum(1 for k in want if SUFFIX.sub("", k) in have_norm)
    misses = [want[k] for k in want if k not in have]
    miss_exts = Counter(os.path.splitext(m)[1].lower() for m in misses)
    miss_dirs = Counter("/".join(m.split("/")[:4]) for m in misses)
    report = {
        "catalog": catalog,
        "export_root": export_root,
        "catalog_bundle_entries": len(bundle_ids),
        "catalog_asset_entries": len(want),
        "exported_keys": len(have),
        "asset_entries_present_in_export": hit,
        "coverage_pct": round(100.0 * hit / max(1, len(want)), 2),
        "asset_entries_present_allowing_dedup_suffix": hit_norm,
        "coverage_pct_allowing_dedup_suffix": round(100.0 * hit_norm / max(1, len(want)), 2),
        "export_file_counts_by_extension": dict(ext_counter),
        "miss_by_extension": dict(miss_exts.most_common(20)),
        "miss_by_dir": dict(miss_dirs.most_common(20)),
        "miss_samples": misses[:40],
        "miss_total": len(misses),
    }
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2, ensure_ascii=False)
    print(json.dumps({k: v for k, v in report.items() if k not in ("miss_samples", "miss_by_dir")}, indent=2))
    print("top miss dirs:")
    for d, c in miss_dirs.most_common(12):
        print(f"  {c:6}  {d}")



if __name__ == "__main__":
    main()
