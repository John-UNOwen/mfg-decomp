// Report 1 (legacy MID phantom profiles): the stored half of the refusal, applied
// directly to a savedata file, with the core not running from it and no restart.
//
//   node .audit/p9_phantom_repair.js [path-to-plugin.db] [--dry-run]
//
// Default path: tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db (the deployment DB the
// running core is serving).
//
// Any savedata path works, including one no core has booted from and any copy made by
// .audit/p9_db_clone.js: those files do not have the _collection generated column yet,
// so the profile filter falls back to the storage space and the run prints which one it
// used (same hardening as .audit/p9_phantom_scan.js).
//
// It is the same update handlers/utils.ts quarantinePhantomProfiles applies when the
// plugin loads - the same $set / $unset on the profile document - reached through
// SQLite instead of the plugin's DB.Update, because the pass at load only runs when
// somebody boots the core. Rows are keyed by _id, so the update does not depend on
// the refid spelling the row happens to be stored under.
//
// NeDB/SqliteStore semantics mirrored on purpose (src/utils/SqliteStore.ts):
//   - applyUpdate: $set writes the path, $unset deletes the key, everything else
//     about the document is left alone.
//   - persist: _id / __s / __refid / createdAt / updatedAt are columns, not JSON
//     fields, so they are stripped from `data` on write; `collection` stays in JSON
//     (the _collection generated column reads it).
//   - timestampData: every update stamps updatedAt (ISO string -> ms column).
//   - WAL + busy_timeout: the core is a live reader/writer on this file.
'use strict';
const fs = require('fs');
const path = require('path');
const { DatabaseSync } = require('node:sqlite');

const DB_PATH = path.resolve(process.argv[2] && !process.argv[2].startsWith('--')
  ? process.argv[2]
  : 'C:/mfg/tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db');
const DRY = process.argv.includes('--dry-run');

// The plugin's own recognising pattern (handlers/utils.ts PHANTOM_REFID_RE):
// 'MID' + String(mid).padStart(8, '0'), and padStart only pads.
const PHANTOM_REFID_RE = /^MID\d{8,}$/;
const normalizeRefid = (s) => String(s || '').trim().toUpperCase();

const db = new DatabaseSync(DB_PATH);
db.exec('PRAGMA journal_mode = WAL');
db.exec('PRAGMA synchronous = NORMAL');
db.exec('PRAGMA busy_timeout = 15000');

// Collection filter. The _collection generated column is added by the core's own store
// open (src/utils/SqliteStore.ts:344-352), so it is missing from exactly the files this
// tool exists for: a savedata no core has booted from, and every copy made by
// .audit/p9_db_clone.js (its CREATE TABLE, p9_db_clone.js:20-22, is the base one).
// Fall back to the storage space - the same hardening p9_phantom_scan.js:12-19 got -
// and print which filter was used. prepare() is where SQLite reports the missing
// column, so one probe settles both queries in this file.
let profileWhere;
let otherCol;
let otherWhere;
let filterUsed;
try {
  db.prepare('SELECT _collection FROM documents LIMIT 0').all();
  profileWhere = "_collection = 'profile'";
  otherCol = '_collection';
  otherWhere = "_collection <> 'profile'";
  filterUsed = "_collection='profile'";
} catch {
  profileWhere = "__s = 'plugins_profile'";
  otherCol = '__s';
  otherWhere = "__s <> 'plugins_profile'";
  filterUsed = "__s='plugins_profile'";
}

const rows = db.prepare(
  `SELECT _id, __s, __refid, createdAt, updatedAt, data FROM documents WHERE ${profileWhere}`
).all();

const pick = (r) => {
  let d;
  try { d = JSON.parse(r.data); } catch { d = null; }
  if (!d || typeof d !== 'object') return null;
  const stored = String(d.refid || '');
  const shape = PHANTOM_REFID_RE.test(normalizeRefid(stored || r.__refid || ''));
  return { _id: r._id, __s: r.__s, __refid: r.__refid, stored, doc: d, shape, already: !!d.phantom_quarantine };
};

const targets = [];
const repaired = [];
const skipped = [];
let scanned = 0;
for (const r of rows) {
  const p = pick(r);
  if (!p) continue;
  scanned++;
  if (!p.shape) continue;
  if (p.already) { skipped.push(p); continue; }
  targets.push(p);
}

const nowUnix = () => Math.floor(Date.now() / 1000);
const UNSET = ['card_id', 'card_issued', 'card_bound', 'card_pin', 'card_updated_at',
  'odekake_publish', 'odekake_grants', 'evicted_sessions', 'music_gacha_reserves',
  'gacha_txn', 'dojo', 'last_daily', 'session_at'];

const updateStmt = db.prepare('UPDATE documents SET data = ?, updatedAt = ? WHERE _id = ?');

for (const p of targets) {
  const at = nowUnix();
  const fromMid = Number(p.doc.player_id || 0) || 0;
  const statesKept = Object.keys(p.doc.states || {}).length;
  const before = {
    name: p.doc.name, player_id: p.doc.player_id, session_id: p.doc.session_id ?? null,
    states: statesKept, card_id: p.doc.card_id ?? null, card_bound: p.doc.card_bound ?? null,
    card_pin: p.doc.card_pin ?? null, odekake_publish: p.doc.odekake_publish ? 'present' : null,
    dojo: Array.isArray(p.doc.dojo) ? p.doc.dojo.length : (p.doc.dojo ?? null),
    evicted_sessions: Array.isArray(p.doc.evicted_sessions) ? p.doc.evicted_sessions.length : (p.doc.evicted_sessions ?? null),
  };

  // applyUpdate($set) then applyUpdate($unset), on a clone, exactly as SqliteStore does.
  const next = JSON.parse(JSON.stringify(p.doc));
  next.refid = normalizeRefid(p.stored || p.__refid || '');
  next.name = 'GUEST';
  next.player_id = 0;
  next.session_id = '';
  next.updated_at = at;
  next.phantom_quarantine = { at, from_mid: fromMid, states_kept: statesKept };
  for (const key of UNSET) delete next[key];

  if (DRY) { repaired.push({ p, at, fromMid, statesKept, before, dry: true }); continue; }

  const persist = { ...next };
  delete persist._id; delete persist.__s; delete persist.__refid;
  delete persist.createdAt; delete persist.updatedAt;
  updateStmt.run(JSON.stringify(persist), Date.now(), p._id);
  repaired.push({ p, at, fromMid, statesKept, before });
}

console.log(`db            : ${DB_PATH}${DRY ? '  [dry run - nothing written]' : ''}`);
console.log(`profile filter: ${filterUsed}`);
console.log(`profile docs  : ${scanned}`);
console.log(`MID-prefixed  : ${targets.length + skipped.length}  (repair now: ${repaired.length}, already repaired: ${skipped.length})`);
for (const t of repaired) {
  const b = t.before;
  console.log(`  ${t.p.stored.padEnd(14)} _id=${t.p._id} mid=${String(t.p.doc.player_id).padEnd(11)} name=${String(b.name).padEnd(8)} session=${JSON.stringify(b.session_id)} states=${b.states} card_id=${JSON.stringify(b.card_id)} card_bound=${JSON.stringify(b.card_bound)} card_pin=${JSON.stringify(b.card_pin)} odekake_publish=${JSON.stringify(b.odekake_publish)} dojo=${JSON.stringify(b.dojo)} evicted=${JSON.stringify(b.evicted_sessions)}`);
  console.log(`    -> player_id=0 session_id="" name=GUEST phantom_quarantine=${JSON.stringify({ at: t.at, from_mid: t.fromMid, states_kept: t.statesKept })}${t.dry ? '  (dry)' : ''}`);
}
for (const s of skipped) {
  console.log(`  ${s.stored.padEnd(14)} already repaired: phantom_quarantine=${JSON.stringify(s.doc.phantom_quarantine)} player_id=${JSON.stringify(s.doc.player_id)} session_id=${JSON.stringify(s.doc.session_id ?? null)}`);
}
console.log(`quarantined   : ${DRY ? repaired.length + ' (would be)' : repaired.length}`);

// Undo record: the payload each repaired row had before this run, so the repair is
// reversible without a full savedata backup.
if (!DRY && repaired.length) {
  const undo = repaired.map((t) => ({ _id: t.p._id, refid: t.p.stored, data: t.p.doc }));
  const file = path.join(__dirname, '..', 'tmp', 'p9repair', `undo-${Date.now()}.json`);
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, JSON.stringify(undo, null, 1), 'utf8');
  console.log(`undo record   : ${file} (${undo.length} row payload(s) as they were before this pass)`);
}

// Nothing else in this savedata may name the same refid as an account.
const others = db.prepare(
  `SELECT ${otherCol}, __refid, count(*) n FROM documents WHERE __refid LIKE 'MID%' AND ${otherWhere} GROUP BY ${otherCol}, __refid`
).all();
console.log(`non-profile rows naming a MID refid: ${others.length ? JSON.stringify(others) : 'none'}`);
db.close();
