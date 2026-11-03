// Report 1 evidence scan: every profile document in a lane's savedata, with the
// phantom shapes side by side (exactly-8-digit = what isPhantomRefid matched
// before the fix; the MID + any-alphanumeric family = what isPhantomRefid matches
// now — the fork's padStart minting (digits, 8 or more) plus probe-minted letter
// spellings like MIDGUESS0001).
// usage: node .audit/p9_phantom_scan.js <path-to-plugin.db>
const path = process.argv[2] || 'tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db';
const { DatabaseSync } = require('node:sqlite');
const db = new DatabaseSync(path, { open: true, readOnly: true });
// Profile rows are the plugin's own collection. A deployment DB carries a
// _collection column for them; a row-by-row clone (.audit/p9_db_clone.js) does not,
// so the filter falls back to the storage space and prints which one it used.
let rows, filterUsed;
try {
  rows = db.prepare(`SELECT _id, __s, __refid, data FROM documents WHERE _collection = 'profile'`).all();
  filterUsed = "_collection='profile'";
} catch {
  rows = db.prepare(`SELECT _id, __s, __refid, data FROM documents WHERE __s = 'plugins_profile'`).all();
  filterUsed = "__s='plugins_profile'";
}
const allRows = db.prepare(`SELECT _id FROM documents`).all();
const EXACT8 = /^MID\d{8}$/;
const ORPHAN = /^MID[A-Z0-9]+$/;
const out = [];
for (const r of rows) {
  let d;
  try { d = JSON.parse(r.data); } catch { d = {}; }
  const refid = String(d.refid || '');
  out.push({
    _id: r._id,
    _s: r.__s,
    __refid: r.__refid,
    refid,
    mid: d.player_id,
    name: d.name,
    session: d.session_id ? String(d.session_id).slice(0, 8) + '...' : '',
    states: Object.keys(d.states || {}).length,
    exact8: EXACT8.test(refid),
    orphan: ORPHAN.test(refid),
    quarantine: !!(d.phantom_quarantine),
  });
}
const orphans = out.filter(o => o.orphan);
console.log(`db            : ${path}`);
console.log(`rows total    : ${allRows.length} (profile filter: ${filterUsed})`);
console.log(`profile docs  : ${out.length}`);
console.log(`MID-prefixed  : ${orphans.length}  (matched by old /^MID\\d{8}$/: ${out.filter(o => o.exact8).length}; MISSED by it: ${orphans.filter(o => !o.exact8).length})`);
console.log(`quarantined   : ${out.filter(o => o.quarantine).length}`);
for (const o of out) {
  if (!o.orphan) continue;
  console.log(`  ${o.refid.padEnd(14)} __refid=${String(o.__refid).padEnd(14)} mid=${String(o.mid).padEnd(11)} name=${String(o.name).padEnd(8)} states=${String(o.states).padEnd(3)} session=${o.session} quarantine=${o.quarantine}`);
}
const real = out.filter(o => !o.orphan);
console.log(`non-phantom profile docs: ${real.length}`);
for (const o of real.slice(0, 12)) {
  console.log(`  ${o.refid.padEnd(20)} __refid=${String(o.__refid).padEnd(20)} mid=${String(o.mid).padEnd(11)} name=${String(o.name).padEnd(10)} states=${o.states} session=${o.session}`);
}
db.close();
