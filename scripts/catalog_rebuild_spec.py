"""Build the WebGL content-reconstruction spec from the shipped catalog.

Reads the shipped catalog.json (install StreamingAssets), decodes the blob
layout (see catalog_decode.py header), and emits a JSON spec:

{
  "assets": [ { "internalId": "Assets/...png", "addr": "<primary key>",
                "labels": ["...", ...], "bundle": "<hash>.bundle" } ],
  "stats":  { ... },
  "missing": [ { "internalId": ..., "addr": ..., "bundle": ... } ]   # path absent on disk
}

Asset entry primary key = firstKeyIdx. Synthetic dependency keys (plain
integers) and bundle-name keys are dropped from labels. A path is considered
present if PROJECT/assets/<internalId> exists as a file.

Usage: python scripts/catalog_rebuild_spec.py [project_root] [out.json]
"""
import base64
import json
import os
import re
import sys
from collections import defaultdict

INSTALL_CATALOG = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"

MAXU = 0xFFFFFFFF
INT_RE = re.compile(r"^-?\d+$")
BUNDLE_SUFFIX = ".bundle"


def read_int32(b, o):
    return int.from_bytes(b[o:o + 4], "little", signed=True)


def decode_object(b, o):
    tag = b[o]
    if tag == 0:
        ln = read_int32(b, o + 1)
        return b[o + 5:o + 5 + ln].decode("ascii", "replace"), 5 + ln
    if tag == 1:
        ln = read_int32(b, o + 1)
        return b[o + 5:o + 5 + ln].decode("utf-16-le", "replace"), 5 + ln
    if tag == 2:
        return int.from_bytes(b[o + 1:o + 3], "little"), 3
    if tag == 3:
        return int.from_bytes(b[o + 1:o + 5], "little"), 5
    if tag == 4:
        return read_int32(b, o + 1), 5
    if tag == 5:
        ln = b[o + 1]
        return b[o + 2:o + 2 + ln].decode("ascii"), 2 + ln
    if tag == 6:
        ln = b[o + 1]
        return b[o + 2:o + 2 + ln].decode("ascii"), 2 + ln
    if tag == 7:
        o2 = o + 1
        al = b[o2]
        o2 += 1 + al
        cl = b[o2]
        o2 += 1 + cl
        jl = read_int32(b, o2)
        o2 += 4
        return {"__json": "json"}, (o2 + jl) - o
    raise ValueError(f"bad object tag {tag} at offset {o}")


def key_record_len(b, o):
    tag = b[o]
    if tag in (0, 1):
        return 5 + read_int32(b, o + 1)
    if tag == 2:
        return 3
    if tag in (3, 4):
        return 5
    if tag in (5, 6):
        return 2 + b[o + 1]
    if tag == 7:
        al = b[o + 1]
        cl = b[o + 2 + al]
        jl = read_int32(b, o + 3 + al + cl)
        return 8 + al + cl + jl
    raise ValueError(f"bad tag {tag}")


def main():
    project = sys.argv[1] if len(sys.argv) > 1 else r"C:\mfg\build\unity-project"
    out = sys.argv[2] if len(sys.argv) > 2 else r"C:\mfgtmp\rebuild_spec.json"

    with open(INSTALL_CATALOG, "r", encoding="utf-8") as f:
        cat = json.load(f)

    internal_ids = cat["m_InternalIds"]
    provider_ids = cat["m_ProviderIds"]
    asset_prov = provider_ids.index(
        "UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider")
    bundle_prov = provider_ids.index(
        "UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider")

    entry_bytes = base64.b64decode(cat["m_EntryDataString"])
    bucket_bytes = base64.b64decode(cat["m_BucketDataString"])
    key_bytes = base64.b64decode(cat["m_KeyDataString"])

    entry_count = read_int32(entry_bytes, 0)
    entries = []
    for i in range(entry_count):
        o = 4 + i * 28
        entries.append([read_int32(entry_bytes, o + k * 4) for k in range(7)])

    key_count = read_int32(key_bytes, 0)
    keys = []
    offsets = []
    o = 4
    while len(keys) < key_count:
        offsets.append(o)
        v, used = decode_object(key_bytes, o)
        keys.append(v if isinstance(v, str) else json.dumps(v))
        o += used
    assert o == len(key_bytes)
    off_to_key = {off: i for i, off in enumerate(offsets)}

    bucket_count = read_int32(bucket_bytes, 0)
    key_entries = defaultdict(list)  # key index -> entry indices
    bundle_entry_keys = defaultdict(set)  # entry index -> bundle-name keys
    o = 4
    for _ in range(bucket_count):
        key_off = read_int32(bucket_bytes, o)
        n = read_int32(bucket_bytes, o + 4)
        idx = [read_int32(bucket_bytes, o + 8 + 4 * j) for j in range(n)]
        ki = off_to_key[key_off]
        key_entries[ki].extend(idx)
        o += 8 + 4 * n

    # bundle entries: keyed by "<name>.bundle"
    entry_bundles = defaultdict(set)  # bundle entry idx -> member asset entry idxs
    for ki, kv in enumerate(keys):
        if not isinstance(kv, str) or not kv.endswith(BUNDLE_SUFFIX):
            continue
        for ei in key_entries[ki]:
            iid, pvid, depk, deph, extra, firstkey, tidx = entries[ei]
            if pvid == bundle_prov:
                continue  # this is the bundle entry itself
            entry_bundles[ei].add(iid if False else entries[depk][0] if depk >= 0 else -2)

    # per asset entry: bundle internal id from dependency key bucket
    def dep_bundle(ei):
        iid, pvid, depk, deph, extra, firstkey, tidx = entries[ei]
        if depk < 0:
            return None
        for de in key_entries[depk]:
            diid, dpvid = entries[de][0], entries[de][1]
            if dpvid == bundle_prov:
                return internal_ids[diid]
        return None

    # group all keys by asset entry
    entry_keys = defaultdict(list)
    for ki, idxs in key_entries.items():
        kv = keys[ki]
        if not isinstance(kv, str):
            continue
        if kv.endswith(BUNDLE_SUFFIX) or INT_RE.match(kv):
            continue  # bookkeeping keys
        for ei in idxs:
            if entries[ei][1] == bundle_prov:
                continue
            entry_keys[ei].append((ki, kv))

    assets = []
    missing = []
    addr_dupes = 0
    seen_addr = set()

    # index the project file tree for exact + fuzzy (AssetRipper '_N' dedupe suffix)
    # path matching, mirroring the rules catalog_coverage.py used.
    exact = set()
    by_norm = {}
    suf_re = re.compile(r"^(?P<base>.*)(?:_\d+)?(?P<ext>\.[A-Za-z0-9]+)$")

    def norm(p):
        m = suf_re.match(p)
        return m.group("base") if m else p

    for root, _, files in os.walk(project):
        for fn in files:
            if fn.endswith(".meta"):
                continue
            full = os.path.join(root, fn)
            rel = os.path.relpath(full, project).replace(os.sep, "/")
            exact.add(rel)
            by_norm.setdefault(norm(rel), rel)

    fuzzy_used = 0
    def resolve_path(iid):
        nonlocal fuzzy_used
        if iid in exact:
            return iid, False
        hit = by_norm.get(norm(iid))
        if hit:
            fuzzy_used += 1
            return hit, True
        return None, False

    for ei, kvs in entry_keys.items():
        iid = internal_ids[entries[ei][0]]
        if iid == "" or not iid.startswith("Assets/"):
            continue
        firstkey = entries[ei][5]
        addr = keys[firstkey] if firstkey < len(keys) else kvs[0][1]
        labels = sorted({kv for _, kv in kvs if kv != addr})
        bpath = dep_bundle(ei)
        bname = bpath.split("\\")[-1] if bpath else None
        real, fuzzy = resolve_path(iid)
        rec = {"internalId": iid, "path": real or iid, "addr": addr,
               "labels": labels, "bundle": bname}
        if real is not None:
            assets.append(rec)
        else:
            missing.append(rec)
        if addr in seen_addr:
            addr_dupes += 1
        seen_addr.add(addr)

    bundles = defaultdict(int)
    for r in assets:
        bundles[r["bundle"] or "?"] += 1
    for r in missing:
        bundles[r["bundle"] or "?"] += 1

    spec = {
        "assets": sorted(assets, key=lambda r: r["path"]),
        "missing": sorted(missing, key=lambda r: r["internalId"]),
        "stats": {
            "asset_entries": len(entry_keys),
            "present": len(assets),
            "fuzzy_mapped": fuzzy_used,
            "missing": len(missing),
            "unique_present_ids": len({r["internalId"] for r in assets}),
            "unique_missing_ids": len({r["internalId"] for r in missing}),
            "bundle_count": len(bundles),
            "label_total": sum(len(r["labels"]) for r in assets + missing),
            "addr_dupes": addr_dupes,
            "missing_bundles": len({r["bundle"] for r in missing}),
        },
    }
    with open(out, "w", encoding="utf-8") as f:
        json.dump(spec, f, ensure_ascii=False)
    base = os.path.splitext(out)[0]
    with open(base + "_assets.tsv", "w", encoding="utf-8", newline="") as f:
        for r in sorted(assets, key=lambda r: r["path"]):
            f.write("\t".join([r["path"], r["addr"], r["bundle"] or "?",
                               "|".join(r["labels"])]) + "\n")
    with open(base + "_missing.tsv", "w", encoding="utf-8", newline="") as f:
        for r in sorted(missing, key=lambda r: r["internalId"]):
            f.write("\t".join([r["internalId"], r["addr"], r["bundle"] or "?",
                               "|".join(r["labels"])]) + "\n")
    print(json.dumps(spec["stats"], indent=1))
    print("wrote", out, "+ tsv siblings")


if __name__ == "__main__":
    main()
