import sqlite3, json, collections, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

db = r"C:\Users\<you>\Documents\GitHub\mfg-decomp\.audit\copy.db"
con = sqlite3.connect(db)
cur = con.cursor()
tables = [r[0] for r in cur.execute("SELECT name FROM sqlite_master WHERE type='table'")]
print("tables:", tables)

kinds = collections.Counter()
profiles = []
for (_id, s, refid, d) in cur.execute("SELECT _id,__s,__refid,data FROM documents ORDER BY _id"):
    if d is None:
        continue
    try:
        o = json.loads(d)
    except Exception:
        print(f"_id={_id} __s={s} refid={refid} UNPARSEABLE data")
        continue
    coll = o.get("collection")
    if coll == "profile":
        st = o.get("states") or {}
        for k in st:
            kinds[k] += 1
        profiles.append((s, refid, o))

print("\n== profile docs:", len(profiles))
for s, refid, o in profiles:
    st = o.get("states") or {}
    print(f"__s={s} refid={refid} name={o.get('name')!r} mid={o.get('player_id')} guest={o.get('is_guest')} "
          f"session={str(o.get('session_id'))[:6]} welcome={o.get('welcome_gifts')} last_daily={o.get('last_daily')} "
          f"fav={o.get('favorite_character')} quar_cc={bool(o.get('quarantine_cc_request'))} keys={sorted(k for k in o if not k.startswith('__'))}")
    for k in sorted(st):
        v = st[k]
        tv = type(v).__name__
        print(f"    {k}: {tv} len={len(v) if isinstance(v,(list,dict,str)) else v}")

print("\n== state kind totals ==")
for k, c in sorted(kinds.items()):
    print(f"  {k}: {c}")
con.close()
