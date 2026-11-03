import { DatabaseSync } from 'node:sqlite';
const db = new DatabaseSync(process.argv[2], { readOnly: true });
const rows = db.prepare('SELECT _id,__s,__refid,createdAt,data FROM documents WHERE __s = ?').all('refid_alias');
const users = db.prepare('SELECT data FROM documents WHERE __s = ?').all('user_account').map((r) => JSON.parse(r.data));
const userByCard = new Map(users.map((u) => [u.cardNumber, u.username]));
console.log('refid_alias total:', rows.length);
for (const r of rows) {
  const d = JSON.parse(r.data);
  console.log(JSON.stringify({ _id: r._id, refid: r.__refid, data: d, ownerCardUser: userByCard.get(r.__refid) || null }));
}
db.close();
