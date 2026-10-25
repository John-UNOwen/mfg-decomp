"""Generate the AddressableAssetsData content state directly as YAML.

The editor API replay (Gate2Content.BuildContentState) is unusable for 28k groups: each
CreateAsset pumps a full import cycle. Instead, a small template capture run (CaptureTemplates)
produced the exact serialization shapes; this script replays them in bulk:

  - copies the captured AddressableAssetsData tree (settings, DefaultObject, profiles,
    data builders, built-in groups + metas) into the project
  - writes one group asset (+ BundledAssetGroupSchema + ContentUpdateGroupSchema + metas,
    deterministic guids from md5) per shipped bundle from rebuild_spec_assets.tsv
  - patches the settings asset text: new group refs into m_GroupAssets, all shipped label
    names into m_LabelTable
  - entries merge per asset guid (shipped multi-bundle membership collapses to one entry;
    extra shipped addresses fold into labels so every runtime key still resolves)

Usage: python scripts/gen_content_state.py [project_root] [spec_tsv] [template_dir]
"""
import hashlib
import json
import os
import re
import shutil
import sys
from collections import OrderedDict

TEMPLATE_ROOT = r"C:\mfgtmp\template"
DEFAULT_LOCAL_GROUP_NAME = "Default Local Group"

GUID_SCRIPT_SETTINGS = "468a46d0ae32c3544b7d98094e6448a9"
GUID_SCRIPT_GROUP = "bbb281ee3bf0b054c82ac2347e9e782c"
GUID_SCRIPT_BGS = "e5d17a21594effb4e9591490b009e7aa"
GUID_SCRIPT_CUG = "5834b5087d578d24c926ce20cd31e6d6"

META_TMPL = """fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

SCHEMA_BGS_TMPL = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {bgs_script}, type: 3}}
  m_Name: {name}_BundledAssetGroupSchema
  m_EditorClassIdentifier: 
  m_Group: {{fileID: 11400000, guid: {group_guid}, type: 2}}
  m_InternalBundleIdMode: 1
  m_Compression: 1
  m_IncludeAddressInCatalog: 1
  m_IncludeGUIDInCatalog: 1
  m_IncludeLabelsInCatalog: 1
  m_InternalIdNamingMode: 0
  m_CacheClearBehavior: 0
  m_IncludeInBuild: 1
  m_BundledAssetProviderType:
    m_AssemblyName: Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
    m_ClassName: UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider
  m_ForceUniqueProvider: 0
  m_UseAssetBundleCache: 1
  m_UseAssetBundleCrc: 1
  m_UseAssetBundleCrcForCachedBundles: 1
  m_UseUWRForLocalBundles: 0
  m_Timeout: 0
  m_ChunkedTransfer: 0
  m_RedirectLimit: -1
  m_RetryCount: 0
  m_BuildPath:
    m_Id: b0aa5af440457f945b1bf58bb84bb6c2
  m_LoadPath:
    m_Id: 445c1f552a363064282662e5782e2517
  m_BundleMode: 0
  m_AssetBundleProviderType:
    m_AssemblyName: Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
    m_ClassName: UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider
  m_BundleNaming: 0
  m_AssetLoadMode: 0
"""

SCHEMA_CUG_TMPL = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {cug_script}, type: 3}}
  m_Name: {name}_ContentUpdateGroupSchema
  m_EditorClassIdentifier: 
  m_Group: {{fileID: 11400000, guid: {group_guid}, type: 2}}
  m_StaticContent: 0
"""

GROUP_HEAD_TMPL = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {group_script}, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier: 
  m_GroupName: {name}
  m_Data:
    m_SerializedData: []
  m_GUID: {mguid}
  m_SerializeEntries:
"""

GROUP_TAIL_TMPL = """  m_ReadOnly: 0
  m_Settings: {{fileID: 11400000, guid: {settings_guid}, type: 2}}
  m_SchemaSet:
    m_Schemas:
    - {{fileID: 11400000, guid: {bgs_guid}, type: 2}}
    - {{fileID: 11400000, guid: {cug_guid}, type: 2}}
"""

ENTRY_TMPL = """  - m_GUID: {guid}
    m_Address: {addr}
    m_ReadOnly: 0
{labels}    FlaggedDuringContentUpdateRestriction: 0
"""

GUID_RE = re.compile(r"^guid: ([0-9a-f]{32})$")


def md5_guid(seed):
    return hashlib.md5(seed.encode("utf-8")).hexdigest()


def esc(s):
    """Quote a YAML scalar when needed (Unity YAML 1.1 flavor)."""
    if s == "":
        return '""'
    need = (s != s.strip() or s[0] in "-?:,[]{}#&*!|>'\"%@`"
            or ": " in s or " #" in s or "\t" in s)
    if not need:
        for c in s:
            if ord(c) < 32:
                need = True
                break
    if not need:
        return s
    out = s.replace("\\", "\\\\").replace('"', '\\"')
    out = out.replace("\n", "\\n").replace("\t", "\\t").replace("\r", "\\r")
    return '"' + out + '"'


def read_guid_for(path_file):
    meta = path_file + ".meta"
    try:
        with open(meta, "r", encoding="utf-8") as f:
            for line in f:
                m = GUID_RE.match(line.strip())
                if m:
                    return m.group(1)
    except OSError:
        pass
    return None


def main():
    project = sys.argv[1] if len(sys.argv) > 1 else r"C:\mfg\build\unity-project"
    spec_tsv = sys.argv[2] if len(sys.argv) > 2 else r"C:\mfgtmp\rebuild_spec_assets.tsv"
    template = sys.argv[3] if len(sys.argv) > 3 else TEMPLATE_ROOT

    # 1. read spec, collapse by asset guid
    assets = {}  # guid -> [path, primary_addr, addrs set, labels set, group name]
    all_labels = set()
    no_guid = 0
    with open(spec_tsv, "r", encoding="utf-8") as f:
        for line in f:
            line = line.rstrip("\n")
            if not line:
                continue
            parts = line.split("\t")
            if len(parts) < 3:
                continue
            path, addr, bundle = parts[0], parts[1], parts[2]
            labels = parts[3].split("|") if len(parts) > 3 and parts[3] else []
            full = os.path.join(project, path.replace("/", os.sep))
            guid = read_guid_for(full)
            if not guid:
                no_guid += 1
                continue
            rec = assets.get(guid)
            if rec is None:
                group = bundle if bundle and bundle != "?" else DEFAULT_LOCAL_GROUP_NAME
                assets[guid] = [path, addr, {addr}, set(labels), group]
            else:
                rec[2].add(addr)
                rec[3].update(labels)
    for rec in assets.values():
        rec[2].discard(rec[1])  # primary address is not its own label
        all_labels.update(rec[2])
        all_labels.update(rec[3])

    groups = OrderedDict()  # group name -> [guid...]
    for guid, rec in sorted(assets.items(), key=lambda kv: kv[1][0]):
        groups.setdefault(rec[4], []).append(guid)

    # 2. copy the captured tree (skip sample group files)
    dst_root = os.path.join(project, "Assets", "AddressableAssetsData")
    src_root = os.path.join(template, "AddressableAssetsData")
    copied = 0
    for root, _, files in os.walk(src_root):
        for fn in files:
            src = os.path.join(root, fn)
            rel = os.path.relpath(src, src_root)
            if fn.startswith("zz_sample_group"):
                continue
            if fn == "AddressableAssetSettings.asset":
                continue  # rewritten below
            dst = os.path.join(dst_root, rel)
            os.makedirs(os.path.dirname(dst), exist_ok=True)
            shutil.copy2(src, dst)
            copied += 1

    # settings guid from its meta (copied unchanged)
    settings_meta = os.path.join(dst_root, "AddressableAssetSettings.asset.meta")
    settings_guid = read_guid_for(os.path.join(dst_root, "AddressableAssetSettings.asset"))
    assert settings_guid, "settings meta missing"

    # 3. write group assets + schemas + metas (skip default local group: patched in place)
    groups_dir = os.path.join(dst_root, "AssetGroups")
    schemas_dir = os.path.join(groups_dir, "Schemas")
    os.makedirs(schemas_dir, exist_ok=True)
    new_group_refs = []  # (name, meta guid) for settings patch
    written = 0
    for name, guids in groups.items():
        g_guid = md5_guid("grp:" + name)
        bgs_guid = md5_guid("bgs:" + name)
        cug_guid = md5_guid("cug:" + name)
        mguid = md5_guid("mg:" + name)
        if name == DEFAULT_LOCAL_GROUP_NAME:
            # append entries to the captured default local group file instead
            dgl_path = os.path.join(groups_dir, name + ".asset")
            with open(dgl_path, "r", encoding="utf-8") as f:
                text = f.read()
            tail_idx = text.index("  m_ReadOnly: 0\n")
            body = []
            for guid in guids:
                rec = assets[guid]
                labels = "".join(
                    "    m_SerializedLabels:\n"
                    + "".join(f"    - {esc(l)}\n" for l in sorted(rec[2] | rec[3]))
                    if (rec[2] | rec[3]) else "")
                body.append(ENTRY_TMPL.format(guid=guid, addr=esc(rec[1]), labels=labels))
            new_text = text[:tail_idx] + "".join(body) + text[tail_idx:]
            with open(dgl_path, "w", encoding="utf-8", newline="") as f:
                f.write(new_text)
            continue
        with open(os.path.join(groups_dir, name + ".asset"), "w", encoding="utf-8", newline="") as f:
            f.write(GROUP_HEAD_TMPL.format(group_script=GUID_SCRIPT_GROUP, name=esc(name), mguid=mguid))
            for guid in guids:
                rec = assets[guid]
                labels_set = rec[2] | rec[3]
                labels = ""
                if labels_set:
                    labels = ("    m_SerializedLabels:\n"
                              + "".join(f"    - {esc(l)}\n" for l in sorted(labels_set)))
                f.write(ENTRY_TMPL.format(guid=guid, addr=esc(rec[1]), labels=labels))
            f.write(GROUP_TAIL_TMPL.format(settings_guid=settings_guid,
                                           bgs_guid=bgs_guid, cug_guid=cug_guid))
            written += 1
        with open(os.path.join(groups_dir, name + ".asset.meta"), "w", encoding="utf-8", newline="") as f:
            f.write(META_TMPL.format(guid=g_guid))
        with open(os.path.join(schemas_dir, name + "_BundledAssetGroupSchema.asset"),
                  "w", encoding="utf-8", newline="") as f:
            f.write(SCHEMA_BGS_TMPL.format(bgs_script=GUID_SCRIPT_BGS, name=esc(name), group_guid=g_guid))
        with open(os.path.join(schemas_dir, name + "_BundledAssetGroupSchema.asset.meta"),
                  "w", encoding="utf-8", newline="") as f:
            f.write(META_TMPL.format(guid=bgs_guid))
        with open(os.path.join(schemas_dir, name + "_ContentUpdateGroupSchema.asset"),
                  "w", encoding="utf-8", newline="") as f:
            f.write(SCHEMA_CUG_TMPL.format(cug_script=GUID_SCRIPT_CUG, name=esc(name), group_guid=g_guid))
        with open(os.path.join(schemas_dir, name + "_ContentUpdateGroupSchema.asset.meta"),
                  "w", encoding="utf-8", newline="") as f:
            f.write(META_TMPL.format(guid=cug_guid))
        new_group_refs.append((name, g_guid))

    # 4. patch settings: group refs + label table
    tpl_settings = os.path.join(src_root, "AddressableAssetSettings.asset")
    with open(tpl_settings, "r", encoding="utf-8") as f:
        lines = f.read().split("\n")
    out = []
    groups_marker = "  m_BuildSettings:"
    labels_start = "  m_LabelTable:"
    labels_end = "  m_GroupTemplateObjects:"
    sample_guid = read_guid_for(os.path.join(src_root, "AssetGroups", "zz_sample_group.asset"))
    phase = None
    for line in lines:
        if sample_guid and sample_guid in line and line.startswith("  - {fileID:"):
            continue  # drop the sample group ref from m_GroupAssets
        if line == groups_marker:
            for _, g in new_group_refs:
                out.append(f"  - {{fileID: 11400000, guid: {g}, type: 2}}")
        if line == labels_start:
            phase = "labels"
            out.append(line)
            out.append("    m_LabelNames:")
            out.append("    - default")
            for lab in sorted(all_labels):
                out.append(f"    - {esc(lab)}")
            continue
        if phase == "labels":
            if line == labels_end:
                phase = None
                out.append(line)
            elif line.strip() == "m_LabelNames:" or line.startswith("    - "):
                pass  # replaced by the regenerated block above
            else:
                phase = None
                out.append(line)
            continue
        out.append(line)
    with open(os.path.join(dst_root, "AddressableAssetSettings.asset"), "w", encoding="utf-8", newline="") as f:
        f.write("\n".join(out))

    summary = {
        "asset_entries": len(assets),
        "groups_new": len(new_group_refs),
        "labels_new": len(all_labels),
        "rows_without_guid": no_guid,
        "template_files_copied": copied,
        "group_files_written": written,
    }
    with open(r"C:\mfgtmp\gen_summary.json", "w", encoding="utf-8") as f:
        json.dump(summary, f, indent=1)
    with open(r"C:\mfgtmp\gen_counts.txt", "w", encoding="utf-8") as f:
        f.write(f"groups={len(new_group_refs)} entries={len(assets)} labels={len(all_labels)}\n")
    print(json.dumps(summary, indent=1))


if __name__ == "__main__":
    main()
