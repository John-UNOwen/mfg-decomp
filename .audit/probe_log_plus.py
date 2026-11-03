import re, base64, json, sys, io, collections
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
LOG = r"C:\Users\<you>\Documents\GitHub\mfg-decomp\mock\mock.log.txt"

kinds = collections.Counter()
plus_raw = 0
plus_esc = 0
samples = []
with open(LOG, "r", encoding="utf-8", errors="replace") as f:
    for line in f:
        if "client_state_write" not in line:
            continue
        m = re.search(r"kind=([a-z_]+)&data=([^&\s]*)", line)
        if not m:
            continue
        kind, data = m.group(1), m.group(2)
        kinds[kind] += 1
        has_raw_plus = "+" in data
        has_esc_plus = "%2B" in data or "%2b" in data
        if has_raw_plus:
            plus_raw += 1
        if has_esc_plus:
            plus_esc += 1
        if (has_raw_plus or has_esc_plus) and len(samples) < 5:
            samples.append((kind, data[:160]))

print("client_state_write kinds seen in real captured traffic:")
for k, c in sorted(kinds.items()):
    print(f"  {k}: {c}")
print(f"\nbodies with literal '+': {plus_raw}")
print(f"bodies with escaped %2B: {plus_esc}")
for s in samples:
    kind, d = s
    print("\nsample kind=", kind, "head=", d)
    dec = d.replace("%2B", "+").replace("%2b", "+")
    try:
        txt = base64.b64decode(dec).decode("utf-8", "replace")
        ok = True
        try:
            json.loads(txt)
        except Exception as e:
            ok = f"JSON error: {e}"
        print("   decoded ok:", ok, "| head:", txt[:200])
    except Exception as e:
        print("   base64 decode failed:", e)
