import re, os

HERE = os.path.expandvars(r"C:\Users\%USERNAME%\Documents\GitHub\mfg-decomp\build\_tmp_audit_moutheye")
CB_RE = re.compile(r"cb(\d+)\[(\d+)\]")
MAD_RE = re.compile(r"mad\s+r(\d+)\.[xzy]{1,4}(?:\.[xyzw])?,\s*[^,]+,\s*cb0\[(\d+)\]\.")
SMP_RE = re.compile(r"sample(?:_l|_s)?\s+[^\s]+,\s+([^\s]+),\s+t(\d+)\.")

def load_layout(path):
    maps = {}
    cur = None
    for line in open(path, encoding="utf-8"):
        m = re.match(r"== PASS\[(\d+)\] (m_Prog\w+) cb bindings", line)
        if m:
            cur = (int(m.group(1)), "VS" if "Vertex" in m.group(2) else "PS")
            maps[cur] = {}
            continue
        if not cur:
            continue
        m = re.match(r"-- CB '([^']+)' reg=(\d+)", line)
        if m:
            cbname, reg = m.group(1), int(m.group(2))
            maps[cur][("__cbname", reg)] = cbname
            continue
        m = re.match(r"\s+off=\s*(\d+) slot=\s*(\d+) comp=(\w*) (\S+) dim", line)
        if m:
            slot, comp, nm = int(m.group(2)), m.group(3), m.group(4)
            maps[cur].setdefault((int(reg), slot), []).append(f"{comp}:{nm}" if comp else nm)
    return maps

for name in ("mouth", "eye"):
    maps = load_layout(os.path.join(HERE, f"cb_layout_{name}.txt"))
    for region, kind in ((2, "VS"), (8, "PS")):
        src = os.path.join(HERE, f"{name}_asm{region:02d}.txt")
        raw = open(src, encoding="utf-8").read().rstrip("\x00")
        m = maps.get((0, kind), {})
        out_lines = []
        pending = {}
        tex_of = {}
        for ln in raw.split("\n"):
            note = None
            hits = sorted(set(CB_RE.findall(ln)))
            if hits:
                parts = []
                for reg, slot in hits:
                    reg = int(reg)
                    slot = int(slot)
                    cbn = m.get(("__cbname", reg), f"CB{reg}")
                    names = m.get((reg, slot))
                    if names:
                        parts.append(f"{cbn}[{slot}]=" + ",".join(names))
                    else:
                        parts.append(f"{cbn}[{slot}]")
                note = "  ; " + "  ".join(parts)
            if kind == "PS":
                mm = MAD_RE.search(ln)
                if mm:
                    dst, slot = int(mm.group(1)), int(mm.group(2))
                    names = m.get((0, slot))
                    st = [x.split(":", 1)[1] for x in (names or []) if x.endswith("_ST")]
                    if st:
                        pending[dst] = st[0][:-3]
                ms = SMP_RE.search(ln)
                if ms:
                    cr, tslot = ms.group(1), int(ms.group(2))
                    crn = int(re.match(r"r(\d+)", cr).group(1)) if cr.startswith("r") else None
                    if crn is not None and crn in pending:
                        tex_of[tslot] = pending[crn]
                    if tslot in tex_of:
                        note = (note or "  ;") + f"  t{tslot}={tex_of[tslot]}"
            out_lines.append(ln + (note or ""))
        dstp = os.path.join(HERE, f"{name}_asm{region:02d}_ann.txt")
        open(dstp, "w", encoding="utf-8").write("\n".join(out_lines) + "\n")
        print("wrote", os.path.basename(dstp), "tex map:", {f"t{k}": v for k, v in sorted(tex_of.items())})
