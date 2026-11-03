"""Verify the build-identity guard in smoke_common: which build a sweep is allowed to claim.

Reproduces the failure the fix targets: a probe pointed at a directory that holds an old copy
of the plugin, and prints SMOKE OK about it. The guard must refuse that, and must still accept a
lane staged from the canonical tree.
"""
import json
import os
import shutil
import sys

sys.path.insert(0, r"C:\mfg\.audit")
from smoke_common import (build_identity, canonical_port, canonical_tree_digest, CORE_ROOT,
                          find_staged_lane, plugin_tree_hashes)

canon = plugin_tree_hashes()
print(f"canonical     {canonical_port()} / {len(canon)} files, "
      f"aog {canon['handlers/aog.ts'][:12]} utils {canon['handlers/utils.ts'][:12]}")

b = build_identity("http://127.0.0.1:8083/aog/")
print(f"8083  -> ok={b['ok']} kind={b['kind']} aog={canon['handlers/aog.ts'][:12]}")

lane = find_staged_lane(8093)
if lane is None:
    print("8093  -> no staged lane; run pwsh scripts/ryunet_stage.ps1 -Action lane")
    sys.exit(1)
b = build_identity("http://127.0.0.1:8093/aog/")
print(f"8093  -> ok={b['ok']} kind={b['kind']} dir={b.get('lane_dir')} staged_at={lane['staged_at']}")

# A lane holding the pre-fix build: same layout, old aog.ts (the 01:06 verify-dist copy).
fake = os.path.join(CORE_ROOT, ".guardtest")
shutil.rmtree(fake, ignore_errors=True)
os.makedirs(fake)
stale_files = dict(lane["plugin_files"])
stale_files["handlers/aog.ts"] = "67d907fa6d8e1c00000000000000000000000000000000000000000000000000"
stamp = {"lane": ".guardtest", "staged_at": lane["staged_at"],
         "ports": {"port": 8099, "mfg_http_port": 22439, "matching_port": 5709},
         "plugin_files": stale_files}
with open(os.path.join(fake, "LANE_STAMP.json"), "w", encoding="utf-8") as f:
    json.dump(stamp, f, indent=2)
b = build_identity("http://127.0.0.1:8099/aog/")
print(f"8099  -> ok={b['ok']} kind={b['kind']} moved={b.get('moved_files')}")
print(f"        {b.get('detail', '')[:160]}")
stale = b

# The retired 8095 lane (verify-p7): frozen, no stamp, so nothing may claim it.
b8095 = build_identity("http://127.0.0.1:8095/aog/")
print(f"8095  -> ok={b8095['ok']} kind={b8095['kind']}")

shutil.rmtree(fake)

ok = (b8095["ok"] is False and b8095["kind"] == "unstaged"
      and build_identity("http://127.0.0.1:8093/aog/")["ok"] is True
      and stale["kind"] == "stale-lane"
      and "handlers/aog.ts" in stale["moved_files"]
      and build_identity("http://127.0.0.1:8083/aog/")["tree"] == canonical_tree_digest())
print("GUARD OK" if ok else "GUARD FAILED")
sys.exit(0 if ok else 1)
