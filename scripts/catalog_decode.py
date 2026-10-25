"""Decode the shipped Addressables catalog blobs into a flat manifest TSV.

Reproduces ContentCatalogData.ToCatalogSerialized layout (1.21.19):
  entryData:  int32 count, then count * 7 int32:
              [internalIdIdx, providerIdx, depKeyIdx, depHash, extraDataIdx,
               firstKeyIdx, typeIdx(ushort)]
  bucketData: int32 bucketCount, then per bucket:
              keyDataOffset int32, entryCount int32, entryCount * int32 entryIdx
  keyData:    int32 keyCount, then SerializationUtilities object records
              (1-byte ObjectType tag; string = int32 len + bytes; int = 4 bytes;
               Hash128/Type = 1-byte len + ascii; JsonObject = 2 length-prefixed
               ascii names + int32 unicode json len + bytes)

Writes TSV lines: key \t providerId \t internalId \t keyType \t deps
where deps is ';'-joined internalIds of the dependency-set entries.

Usage: python catalog_decode.py [catalog.json] [out.tsv]
"""
import base64
import json
import sys
from collections import Counter

INSTALL_CATALOG = r"\\MYAPPSANDBOX\c\Games\MFG\game\MFGClient_Data\StreamingAssets\aa\catalog.json"

MAXU = 0xFFFFFFFF


def read_int32(b, o):
    return int.from_bytes(b[o:o + 4], "little", signed=True)


def decode_object(b, o):
    """Return (value, type_name, consumed_bytes)."""
    tag = b[o]
    if tag == 0:  # AsciiString
        ln = read_int32(b, o + 1)
        return b[o + 5:o + 5 + ln].decode("ascii", "replace"), "String", 5 + ln
    if tag == 1:  # UnicodeString
        ln = read_int32(b, o + 1)
        return b[o + 5:o + 5 + ln].decode("utf-16-le", "replace"), "String", 5 + ln
    if tag == 2:  # UInt16
        return int.from_bytes(b[o + 1:o + 3], "little"), "UInt16", 3
    if tag == 3:  # UInt32
        return int.from_bytes(b[o + 1:o + 5], "little"), "UInt32", 5
    if tag == 4:  # Int32
        return read_int32(b, o + 1), "Int32", 5
    if tag == 5:  # Hash128
        ln = b[o + 1]
        return b[o + 2:o + 2 + ln].decode("ascii"), "Hash128", 2 + ln
    if tag == 6:  # Type
        ln = b[o + 1]
        return b[o + 2:o + 2 + ln].decode("ascii"), "Type", 2 + ln
    if tag == 7:  # JsonObject
        o2 = o + 1
        al = b[o2]
        asm = b[o2 + 1:o2 + 1 + al].decode("ascii")
        o2 += 1 + al
        cl = b[o2]
        cls = b[o2 + 1:o2 + 1 + cl].decode("ascii")
        o2 += 1 + cl
        jl = read_int32(b, o2)
        o2 += 4
        js = b[o2:o2 + jl].decode("utf-16-le", "replace")
        return {"__json": f"{asm}:{cls}", "json": js}, "JsonObject", (o2 + jl) - o
    raise ValueError(f"bad object tag {tag} at offset {o}")


def main():
    path = sys.argv[1] if len(sys.argv) > 1 else INSTALL_CATALOG
    out = sys.argv[2] if len(sys.argv) > 2 else r"C:\mfgtmp\catalog_manifest_py.tsv"

    with open(path, "r", encoding="utf-8") as f:
        cat = json.load(f)

    internal_ids = cat["m_InternalIds"]
    provider_ids = cat["m_ProviderIds"]
    res_types = [f"{t.get('m_AssemblyName', '?')}:{t.get('m_ClassName', '?')}" for t in cat["m_resourceTypes"]]

    entry_bytes = base64.b64decode(cat["m_EntryDataString"])
    bucket_bytes = base64.b64decode(cat["m_BucketDataString"])
    key_bytes = base64.b64decode(cat["m_KeyDataString"])

    # entries
    entry_count = read_int32(entry_bytes, 0)
    assert len(entry_bytes) == 4 + entry_count * 28, \
        f"entryData size mismatch: {len(entry_bytes)} vs {4 + entry_count * 28}"
    entries = []
    for i in range(entry_count):
        o = 4 + i * 28
        vals = [read_int32(entry_bytes, o + k * 4) for k in range(7)]
        entries.append(vals)

    # keys
    key_count = read_int32(key_bytes, 0)
    keys = []
    o = 4
    while len(keys) < key_count:
        v, tname, used = decode_object(key_bytes, o)
        keys.append((v, tname))
        o += used
    assert o == len(key_bytes), f"keyData trailing bytes: {len(key_bytes) - o}"

    # buckets
    bucket_count = read_int32(bucket_bytes, 0)
    buckets = []
    o = 4
    for _ in range(bucket_count):
        key_off = read_int32(bucket_bytes, o)
        n = read_int32(bucket_bytes, o + 4)
        idx = [read_int32(bucket_bytes, o + 8 + 4 * j) for j in range(n)]
        buckets.append((key_off, idx))
        o += 8 + 4 * n
    assert o == len(bucket_bytes), f"bucketData trailing bytes: {len(bucket_bytes) - o}"

    # key data offsets -> key index (offsets are in keyData byte space; keys were
    # written in order, so map offsets by walking the same order)
    offsets = []
    o = 4
    for v, tname in keys:
        offsets.append(o)
        # recompute record length from the encoded object
        tag = key_bytes[o]
        if tag in (0, 1):
            o += 5 + read_int32(key_bytes, o + 1)
        elif tag == 2:
            o += 3
        elif tag in (3, 4):
            o += 5
        elif tag in (5, 6):
            o += 2 + key_bytes[o + 1]
        elif tag == 7:
            al = key_bytes[o + 1]
            cl = key_bytes[o + 2 + al]
            jl = read_int32(key_bytes, o + 3 + al + cl)
            o += 8 + al + cl + jl
        else:
            raise ValueError(f"bad tag {tag}")
    assert o == len(key_bytes), "offset walk mismatch"
    off_to_key = {off: i for i, off in enumerate(offsets)}

    # bucket per key index -> entry index lists
    key_buckets = [[] for _ in keys]
    for key_off, idx in buckets:
        key_buckets[off_to_key[key_off]].append(idx)

    # dependency sets: depKeyIdx points to a key whose bucket lists the dep entries
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write("#key\tproviderId\tinternalId\tkeyType\tdeps\n")
        stats = Counter()
        for ki, (kval, ktype) in enumerate(keys):
            for ei in (e for bucket in key_buckets[ki] for e in bucket):
                iid, pvid, depk, deph, extra, firstkey, tidx = entries[ei]
                if depk < 0:
                    deps = ""
                else:
                    parts = []
                    for bucket in key_buckets[depk]:
                        for de in bucket:
                            parts.append(";")
                            parts.append(internal_ids[entries[de][0]])
                    deps = "".join(parts)
                iid_s = internal_ids[iid] if iid != MAXU and iid >= 0 else ""
                pv_s = provider_ids[pvid] if 0 <= pvid < len(provider_ids) else ""
                k = str(kval)
                if ktype == "JsonObject":
                    k = kval["__json"]
                k = k.replace("\t", " ").replace("\n", " ")
                f.write(f"{k}\t{pv_s}\t{iid_s}\t{ktype}\t{deps}\n")
                stats[ktype] += 1
                stats["provider" + pv_s[:8]] += 1

    print(f"entries={entry_count} keys={key_count} buckets={bucket_count}")
    print("key types:", dict(Counter(t for _, t in keys)))
    print("top providers:", [x for x in stats.most_common() if x[0].startswith("provider")][:6])
    str_keys = [k for k, t in keys if t in ("String",)]
    print(f"string keys: {len(str_keys)}")
    for s in str_keys[:8]:
        print("  sample:", s)
    print("int keys sample:", [k for k, t in keys if t in ("Int32", "UInt32")][:8])
    print("wrote", out)


if __name__ == "__main__":
    main()
