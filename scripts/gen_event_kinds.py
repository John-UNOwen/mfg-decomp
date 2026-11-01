#!/usr/bin/env python3
"""Regenerate the plugin's data/event_kinds.json in the canonical tree
(tmp/RyuNET-core/dist/plugins/mfg@asphyxia - the only copy the core loads).

The list is the client's own enumeration: every member of GameEventType
(api/recovered/Assembly-CSharp/GameEventType.cs), in declaration order. The
front server has to answer appli_info kind="events" with a row per name,
because GameEventStatus.OnAfterDeserialize keys its lookup table on the enum
and GameEventData.OnAfterDeserialize drops any row whose "name" does not
parse to a GameEventType member - a name the server never sends is a kind the
server never schedules.

active is left to the plugin's own config (BASE_EVENTS + the CostumeAddition
range + the event-taku set + VFG_EVENTS_ACTIVE); the file only records the
kind vocabulary and the params the server backs by default.
"""
import json
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, "api", "recovered", "Assembly-CSharp", "GameEventType.cs")
# Canonical tree only: pwsh scripts/ryunet_stage.ps1 -Action mirror refreshes the
# tmp/asphyxia_plugins staging clone from it. Writing here used to land where no server reads.
OUT = os.path.join(ROOT, "tmp", "RyuNET-core", "dist", "plugins", "mfg@asphyxia", "data",
                   "event_kinds.json")

text = open(SRC, encoding="utf-8").read()
names = re.findall(r"^\s+([A-Za-z_][A-Za-z0-9_]*),?$", text, re.M)
if not names:
    raise SystemExit("no enum members parsed")

doc = {
    "_source": "api/recovered/Assembly-CSharp/GameEventType.cs (client enumeration, declaration order)",
    "kinds": names,
}
os.makedirs(os.path.dirname(OUT), exist_ok=True)
with open(OUT, "w", encoding="utf-8") as fh:
    json.dump(doc, fh, indent=1, ensure_ascii=False)
    fh.write("\n")
print(f"{OUT}: {len(names)} GameEventType kinds")
