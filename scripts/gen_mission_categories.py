#!/usr/bin/env python3
"""Regenerate the plugin's data/mission_categories.json in the canonical tree
(tmp/RyuNET-core/dist/plugins/mfg@asphyxia - the only copy the core loads).

MissionStatus.OnAfterDeserialize keys its schedule table on MFG.Types.Mission.Category
and MissionData.OnAfterDeserialize drops any row whose "category" does not parse to a
Category member, so the schedule has to carry a row per category the client can ask
about - not just the ones this server runs. Dates for the dated event categories are
the far window the plugin already uses (no real schedule); active stays false for them
and VFG_MISSION_ACTIVE turns individual categories on.
"""
import json
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, "api", "recovered", "Assembly-CSharp", "MFG.Types.Mission", "Category.cs")
# Canonical tree only: pwsh scripts/ryunet_stage.ps1 -Action mirror refreshes the
# tmp/asphyxia_plugins staging clone from it.
OUT = os.path.join(ROOT, "tmp", "RyuNET-core", "dist", "plugins", "mfg@asphyxia", "data",
                   "mission_categories.json")

ACTIVE_DEFAULT = {"JudgmentDaily", "JudgmentWeekly", "Player", "Character", "Weekly", "Yaku", "StartDash"}

text = open(SRC, encoding="utf-8").read()
names = re.findall(r"^\s+([A-Za-z_][A-Za-z0-9_]*),?$", text, re.M)
if not names:
    raise SystemExit("no enum members parsed")

rows = [{"id": i, "name": n, "active": n in ACTIVE_DEFAULT} for i, n in enumerate(names)]
doc = {
    "_source": "api/recovered/Assembly-CSharp/MFG.Types.Mission/Category.cs (declaration order, id == enum int)",
    "categories": rows,
}
os.makedirs(os.path.dirname(OUT), exist_ok=True)
with open(OUT, "w", encoding="utf-8") as fh:
    json.dump(doc, fh, indent=1, ensure_ascii=False)
    fh.write("\n")
on = [r["name"] for r in rows if r["active"]]
print(f"{OUT}: {len(rows)} categories, {len(on)} active by default: {', '.join(on)}")
