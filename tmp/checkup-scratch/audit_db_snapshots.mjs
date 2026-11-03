import fs from 'node:fs';
import path from 'node:path';
import { DatabaseSync } from 'node:sqlite';

const roots = ['C:/mfg/tmp/RyuNET-core/.frozen-builds', 'C:/mfg/tmp'];
const found = [];
function scanDir(dir, depth = 0) {
  if (depth > 6) return;
  let entries = [];
  try { entries = fs.readdirSync(dir, { withFileTypes: true }); } catch { return; }
  for (const e of entries) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) {
      if (e.name === 'node_modules') continue;
      scanDir(p, depth + 1);
    } else if (e.name.endsWith('.db') && !e.name.includes('@asphyxia')) {
      found.push(p);
    }
  }
}
for (const r of roots) scanDir(r);
console.log('db files found:', found.length);
for (const f of found) {
  let db;
  try { db = new DatabaseSync(f, { readOnly: true }); } catch (e) { console.log('SKIP ' + f + ' ' + e.message); continue; }
  try {
    const rows = db.prepare("SELECT data FROM documents WHERE __s='user_account'").all();
    const names = rows.map((r) => { try { return JSON.parse(r.data).username; } catch { return null; } }).filter(Boolean);
    const mixed = names.filter((n) => typeof n === 'string' && n !== n.toLowerCase());
    const dupLower = (() => { const m = new Map(); for (const n of names) m.set(String(n).toLowerCase(), (m.get(String(n).toLowerCase()) || 0) + 1); return [...m].filter(([, c]) => c > 1); })();
    if (names.length) console.log(JSON.stringify({ db: f.replace('C:/mfg/', ''), users: names.length, mixedCase: mixed, dupLower }));
  } catch (e) {
    console.log('ERR ' + f + ': ' + e.message);
  } finally { db.close(); }
}
