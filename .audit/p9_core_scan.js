// Core-side view of the phantom refids: does core.db carry a profile or a
// refid_alias for them (that is what makes a plugin DB.Upsert land), and are
// there any MID-shaped rows there too.
// usage: node .audit/p9_core_scan.js [core.db] [term]
const path = process.argv[2] || 'tmp/RyuNET-core/dist/savedata/core.db';
const term = process.argv[3] || 'MID';
const { DatabaseSync } = require('node:sqlite');
const db = new DatabaseSync(path, { open: true, readOnly: true });
const rows = db.prepare('SELECT _id, __s, __refid, data FROM documents ORDER BY __s, __refid').all();
console.log(`db   : ${path}`);
console.log(`docs : ${rows.length}`);
const bySpace = {};
for (const r of rows) bySpace[r.__s] = (bySpace[r.__s] || 0) + 1;
console.log('spaces: ' + Object.entries(bySpace).map(([k, v]) => `${k}=${v}`).join(', '));
const orphans = rows.filter((r) => /^MID\d{8,}$/.test(String(r.__refid || '')));
console.log(`MID-prefixed core rows: ${orphans.length}`);
for (const r of orphans) console.log(`  __s=${r.__s} __refid=${r.__refid} data=${String(r.data).slice(0, 160)}`);
const hit = rows.filter((r) => String(r.__refid || '').includes(term));
console.log(`rows whose __refid contains "${term}": ${hit.length}`);
for (const r of hit.slice(0, 20)) console.log(`  __s=${r.__s} __refid=${r.__refid} data=${String(r.data).slice(0, 140)}`);
db.close();
