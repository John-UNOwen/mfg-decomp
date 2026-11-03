import { DatabaseSync } from 'node:sqlite';
const db = new DatabaseSync('C:/mfg/tmp/RyuNET-core/dist/savedata/core.db', { readOnly: true });
const u = db.prepare("SELECT createdAt,data FROM documents WHERE __s='user_account'").all().map(r => ({ created: r.createdAt, ...JSON.parse(r.data) }));
console.log('count:', u.length, 'mixed-case:', u.filter(x => x.username !== x.username.toLowerCase()).map(x => x.username));
const dup = (() => { const m = new Map(); for (const x of u) { const k = String(x.username).toLowerCase(); m.set(k, (m.get(k)||0)+1 ); } return [...m].filter(([,c]) => c>1); })();
console.log('case-insensitive duplicates:', dup);
console.log('newest 3:', u.sort((a,b)=>(b.created||0)-(a.created||0)).slice(0,3).map(x=>({u:x.username,c:x.cardNumber})));
db.close();
