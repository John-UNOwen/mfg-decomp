import { DatabaseSync } from 'node:sqlite';
const db = new DatabaseSync('C:/mfg/tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db', { readOnly: true });
console.log(db.prepare("SELECT name FROM sqlite_master WHERE type='table'").all());
console.log(db.prepare('SELECT __s, COUNT(*) c FROM documents GROUP BY __s ORDER BY c DESC').all().slice(0,40));
const rows = db.prepare("SELECT _id,__s,__refid,data FROM documents WHERE data LIKE '%username%' LIMIT 30").all();
console.log('docs containing the word username:', rows.length);
for (const r of rows) console.log(JSON.stringify({ __s: r.__s, refid: r.__refid, data: r.data.slice(0, 300) }));
db.close();
