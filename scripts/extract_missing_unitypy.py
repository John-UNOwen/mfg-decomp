"""UnityPy fallback: extract the asset classes AssetRipper Free 2.0.0 does not export.

AssetRipper's export path emits Texture2D/Sprite/SpriteAtlas/AudioClip/Mesh/Font/Shader/
TextAsset/scene+prefab geometry, but silently skips Material (.mat), AnimationClip (.anim),
AnimatorController (.controller) and AnimatorOverrideController (.overridecontroller) that
live only inside the shipped addressable bundles. This script opens exactly the bundles that
hold those missing entries (resolved from aa/catalog.json) and dumps each object to JSON plus
raw serialized bytes, with a manifest that records bundle + PathID + original asset path.

Usage:
  python scripts/extract_missing_unitypy.py [--limit N] [--out build/unity-project-missing]
      [--catalog <path>] [--export-root build/unity-project] [--dry-run]

Reads are from the read-only install share; all writes go under --out (default
build/unity-project-missing) inside the repo.
"""
import argparse
import json
import os
import sys
from collections import Counter, defaultdict

INSTALL_CATALOG = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"
INSTALL_BUNDLES = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\StandaloneWindows64"
PREFIX = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}\\StandaloneWindows64\\"
GAP_EXTENSIONS = {".anim", ".mat", ".controller", ".overridecontroller", ".asset"}

# extension -> UnityPy/Unity object type names to look for inside the bundle
TYPE_FOR_EXT = {
    ".anim": {"AnimationClip"},
    ".mat": {"Material"},
    ".controller": {"AnimatorController", "RuntimeAnimatorController"},
    ".overridecontroller": {"AnimatorOverrideController"},
    ".asset": {"MonoBehaviour", "ScriptableObject", "GameObject", "MonoScript"},
}


def strip_ext(p):
    return os.path.splitext(p)[0]


def load_missing(catalog_path, export_root):
    """Return {bundle_file: {asset_path: ext}} for catalog entries AssetRipper did not export."""
    with open(catalog_path, "r", encoding="utf-8") as f:
        cat = json.load(f)
    have = set()
    for root, _dirs, files in os.walk(os.path.join(export_root, "Assets")):
        for fn in files:
            rel = os.path.relpath(os.path.join(root, fn), export_root).replace("\\", "/")
            body = rel[:-5] if rel.lower().endswith(".json") else rel
            for key in (strip_ext(body).lower(), os.path.splitext(strip_ext(body))[0].lower()):
                have.add(key)
    per_bundle = defaultdict(dict)
    total = 0
    for raw in cat["m_InternalIds"]:
        if not isinstance(raw, str):
            continue
        s = raw[len(PREFIX):] if raw.startswith(PREFIX) else raw
        s = s.replace("\\", "/")
        if s.lower().endswith(".bundle"):
            continue
        idx = s.find(".bundle/")
        if idx < 0:
            continue
        bundle = s[: idx + len(".bundle")]
        apath = s[idx + len(".bundle/"):]
        ext = os.path.splitext(apath)[1].lower()
        if ext not in GAP_EXTENSIONS:
            continue
        if strip_ext(apath).lower() in have:
            continue
        per_bundle[os.path.basename(bundle)][apath] = ext
        total += 1
    return per_bundle, total


def dump_object(obj, out_dir, stem, manifest_rows, bundle_name, apath, ext, node=None):
    from UnityPy.helpers import TypeTreeHelper

    os.makedirs(out_dir, exist_ok=True)
    row = {
        "asset_path": apath,
        "extension": ext,
        "bundle": bundle_name,
        "path_id": int(getattr(obj, "path_id", 0)),
        "type": getattr(obj.type, "name", None) or str(obj.type),
        "name": str(getattr(node, "m_Name", "") or ""),
    }
    for dump_name in ("DumpAttribute", "DumpValues"):
        fn = getattr(TypeTreeHelper, dump_name, None)
        if fn is None:
            continue
        try:
            attr = fn(node if node is not None else obj)
            with open(os.path.join(out_dir, stem + ".json"), "w", encoding="utf-8") as f:
                json.dump(attr, f, indent=1, ensure_ascii=False, default=str)
            row["json"] = os.path.relpath(os.path.join(out_dir, stem + ".json"), out_dir)
            break
        except Exception as exc:  # noqa: BLE001 - try the next dump helper
            row[f"{dump_name}_error"] = repr(exc)[:200]

    try:
        raw = obj.get_raw_data()
        with open(os.path.join(out_dir, stem + ".bin"), "wb") as f:
            f.write(raw)
        row["bin_bytes"] = len(raw)
    except Exception as exc:  # noqa: BLE001
        row["bin_error"] = repr(exc)[:200]
    manifest_rows.append(row)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--catalog", default=INSTALL_CATALOG)
    ap.add_argument("--bundles-dir", default=INSTALL_BUNDLES)
    ap.add_argument("--export-root", default=r"build\unity-project")
    ap.add_argument("--out", default=r"build\unity-project-missing")
    ap.add_argument("--limit", type=int, default=0, help="limit number of bundles (smoke test)")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    per_bundle, total_missing = load_missing(args.catalog, args.export_root)
    bundles = sorted(per_bundle)
    if args.limit:
        bundles = bundles[: args.limit]
    print(f"missing_gap_entries={total_missing} bundles_holding_them={len(bundles)} selected={len(bundles)}")
    if args.dry_run:
        for b in bundles[:10]:
            print(" ", b, list(per_bundle[b].items())[:3])
        return

    import UnityPy

    os.makedirs(args.out, exist_ok=True)
    manifest = []
    wanted_names = defaultdict(dict)  # bundle -> {lowered stem: (apath, ext)}
    for b in bundles:
        for apath, ext in per_bundle[b].items():
            wanted_names[b][os.path.splitext(os.path.basename(apath))[0].lower()] = (apath, ext)

    done = 0
    errors = []
    for n, bundle_name in enumerate(bundles, 1):
        path = os.path.join(args.bundles_dir, bundle_name)
        want = wanted_names[bundle_name]
        want_types = set()
        for _apath, ext in want.values():
            want_types |= TYPE_FOR_EXT[ext]
        try:
            env = UnityPy.load(path)
        except Exception as exc:  # noqa: BLE001
            errors.append((bundle_name, repr(exc)[:200]))
            continue
        try:
            for obj in env.objects:
                tname = getattr(obj.type, "name", None) or str(obj.type)
                if tname not in want_types:
                    continue
                node = None
                try:
                    node = obj.read()
                except Exception:  # noqa: BLE001
                    node = None
                name = str(getattr(node, "m_Name", "") or getattr(obj, "m_Name", "") or "")
                key = name.lower()
                hit = want.get(key)
                if hit is None:
                    continue
                apath, ext = hit
                rel = os.path.dirname(apath.replace("Assets/", "")).replace("/", os.sep)
                out_dir = os.path.join(args.out, "Assets", rel)
                stem = os.path.basename(apath)
                dump_object(obj, out_dir, stem, manifest, bundle_name, apath, ext, node)
                done += 1

        except Exception as exc:  # noqa: BLE001
            errors.append((bundle_name, repr(exc)[:200]))
        finally:
            del env
        if n % 200 == 0:
            print(f"  bundles {n}/{len(bundles)} dumped_objects={done}", flush=True)

    with open(os.path.join(args.out, "manifest.json"), "w", encoding="utf-8") as f:
        json.dump({"bundles_scanned": len(bundles), "objects_dumped": done,
                   "by_extension": dict(Counter(r["extension"] for r in manifest)),
                   "errors": errors[:50], "rows": manifest}, f, indent=1, ensure_ascii=False)
    print(f"DONE bundles={len(bundles)} dumped={done} errors={len(errors)} out={args.out}")


if __name__ == "__main__":
    main()
