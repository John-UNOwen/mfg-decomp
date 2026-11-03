// Adversarial check: dump identity-bearing documents from the live core DB.
// Read-only; prints every user_account plus every collection that references a username.
import { DatabaseSync } from 'node:sqlite';
import path from 'node:path';

const dbPath = process.argv[2] || 'C:/mfg/tmp/RyuNET-core/dist/savedata/core.db';
const db = new DatabaseSync(dbPath, { readOnly: true });

const tables = db.prepare("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name").all();
console.log('TABLES:', tables.map((t) => t.name).join(', '));

for (const t of tables) {
  const cols = db.prepare(`PRAGMA table_info(${t.name})`).all().map((c) => c.name);
  const n = db.prepare(`SELECT COUNT(*) AS c FROM ${t.name}`).get().c;
  console.log(`TABLE ${t.name} cols=[${cols.join(',')}] rows=${n}`);
}

function docs(__s) {
  const rows = db.prepare('SELECT * FROM documents WHERE __s = ?').all(__s);
  return rows;
}

function showDoc(d) {
  const o = {};
  for (const [k, v] of Object.entries(d)) {
    if (k === 'doc' || k === 'data' || k === 'json') continue;
    o[k] = typeof v === 'string' && v.length > 400 ? v.slice(0, 400) + '...' : v;
  }
  return o;
}

console.log('\n=== user_account ===');
const users = docs('user_account');
for (const u of users) {
  console.log(JSON.stringify(showDoc(u)));
}
console.log('user_account count:', users.length);

// duplicate / mixed-case detection on the raw stored value
const rawCol = ['username', 'doc', 'data', 'value', 'payload'].find((c) =>
  db.prepare('PRAGMA table_info(documents)').all().some((x) => x.name === c)
);
console.log('\ndocuments columns:', db.prepare('PRAGMA table_info(documents)').all().map((c) => c.name).join(','));

console.log('\n=== collections mentioning a username field ===');
for (const s of ['cabinet', 'api_token', 'tachi_token', 'oauth_client', 'oauth_code', 'oauth_access_token']) {
  const rows = docs(s);
  console.log(`--- ${s}: ${rows.length}`);
  for (const r of rows.slice(0, 40)) console.log(JSON.stringify(showDoc(r)));
}

console.log('\n=== card / profile docs (reserved-field docs) ===');
for (const s of ['card', 'profile', 'user', 'game_profile', 'plugins_profile']) {
  const rows = docs(s);
  console.log(`--- __s=${s}: ${rows.length}`);
  for (const r of rows.slice(0, 20)) console.log(JSON.stringify(showDoc(r)).slice(0, 600));
}

db.close();
