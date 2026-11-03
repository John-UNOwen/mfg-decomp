// Read-only probe: schema + every MID-prefixed row in the plugin savedata, plus any
// core.db rows that name the same refid (ensureCoreAlias -> EnsurePhantomProfile).
// usage: node .audit/p9_db_probe.js [plugin.db] [core.db]
const path = process.argv[2] || 'tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db';
const corePath = process.argv[3] || 'tmp/RyuNET-core/dist/savedata/core.db';
const { DatabaseSync } = require('node:sqlite');

function dump(dbFile) {
  const db = new DatabaseSync(dbFile, { open: true, readOnly: true });
  console.log(`=== ${dbFile} ===`);
  const tables = db.prepare(`SELECT name, sql FROM sqlite_master WHERE type IN ('table','index')`).all();
  for (const t of tables) console.log(`  ${t.type} ${t.name}`);
  const rows = db.prepare(`SELECT _id, __s, __refid, createdAt, updatedAt, length(data) len, data FROM documents WHERE __refid LIKE 'MID%' OR json_extract(data,'$.refid') LIKE 'MID%'`).all();
  console.log(`  MID-naming rows: ${rows.length}`);
  for (const r of rows) {
    let d = {};
    try { d = JSON.parse(r.data); } catch { }
    const keys = Object.keys(d).sort();
    console.log(`  _id=${r._id} __s=${r.__s} __refid=${r.__refid} updatedAt=${r.updatedAt}`);
    console.log(`    keys: ${keys.join(', ')}`);
    console.log(`    refid=${d.refid} collection=${d.collection} name=${d.name} player_id=${d.player_id} session_id=${JSON.stringify(d.session_id)} phantom_quarantine=${JSON.stringify(d.phantom_quarantine || null)}`);
    console.log(`    card_id=${JSON.stringify(d.card_id ?? null)} card_bound=${JSON.stringify(d.card_bound ?? null)} card_pin=${JSON.stringify(d.card_pin ?? null)} dojo=${Array.isArray(d.dojo) ? d.dojo.length : JSON.stringify(d.dojo ?? null)} states=${Object.keys(d.states || {}).length}`);
  }
  db.close();
}

dump(path);
dump(corePath);
