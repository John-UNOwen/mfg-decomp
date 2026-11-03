// Row-level fingerprint of a savedata file: every document, its identity columns and
// a sha256 of its JSON payload. Used to prove what a repair did and did not touch.
// usage: node .audit/p9_db_fingerprint.js <path-to.db> <out-file>
'use strict';
const fs = require('fs');
const crypto = require('crypto');
const { DatabaseSync } = require('node:sqlite');

const DB_PATH = process.argv[2];
const OUT = process.argv[3];
if (!DB_PATH || !OUT) { console.error('usage: node .audit/p9_db_fingerprint.js <db> <out>'); process.exit(2); }

const db = new DatabaseSync(DB_PATH, { open: true, readOnly: true });
db.exec('PRAGMA busy_timeout = 15000');
const rows = db.prepare('SELECT _id, __s, __refid, createdAt, updatedAt, data FROM documents ORDER BY _id').all();
const lines = [];
const byCollection = {};
for (const r of rows) {
  let coll = null;
  try { coll = JSON.parse(r.data).collection ?? null; } catch { }
  byCollection[coll] = (byCollection[coll] || 0) + 1;
  const h = crypto.createHash('sha256').update(r.data, 'utf8').digest('hex').slice(0, 16);
  lines.push(`${r._id}\t${r.__s}\t${r.__refid}\t${coll}\t${h}`);
}
fs.writeFileSync(OUT, lines.join('\n') + '\n', 'utf8');
console.log(`db      : ${DB_PATH}`);
console.log(`rows    : ${rows.length}  by collection: ${JSON.stringify(byCollection)}`);
console.log(`written : ${OUT}`);
db.close();
