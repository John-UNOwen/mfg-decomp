// Consistent copy of a savedata file row by row, safe to take while the core has the
// original open: everything is read through one read connection (which sees the WAL) and
// written into a fresh file, instead of copying the .db without its .wal.
// usage: node .audit/p9_db_clone.js <src.db> <dst.db>
'use strict';
const fs = require('fs');
const path = require('path');
const { DatabaseSync } = require('node:sqlite');

const [SRC, DST] = [process.argv[2], process.argv[3]];
if (!SRC || !DST) { console.error('usage: node .audit/p9_db_clone.js <src.db> <dst.db>'); process.exit(2); }
fs.rmSync(DST, { force: true });
fs.rmSync(DST + '-wal', { force: true });
fs.rmSync(DST + '-shm', { force: true });

const src = new DatabaseSync(SRC, { open: true, readOnly: true });
src.exec('PRAGMA busy_timeout = 15000');
const dst = new DatabaseSync(DST);
dst.exec('PRAGMA journal_mode = WAL');
dst.exec(`CREATE TABLE IF NOT EXISTS documents (
  _id TEXT PRIMARY KEY, __s TEXT, __refid TEXT, createdAt INTEGER, updatedAt INTEGER, data TEXT NOT NULL
);`);
const rows = src.prepare('SELECT _id, __s, __refid, createdAt, updatedAt, data FROM documents ORDER BY _id').all();
const ins = dst.prepare('INSERT INTO documents (_id, __s, __refid, createdAt, updatedAt, data) VALUES (?, ?, ?, ?, ?, ?)');
dst.exec('BEGIN');
for (const r of rows) ins.run(r._id, r.__s, r.__refid, r.createdAt, r.updatedAt, r.data);
dst.exec('COMMIT');
console.log(`cloned ${rows.length} rows: ${SRC} -> ${DST}`);
src.close();
dst.close();
