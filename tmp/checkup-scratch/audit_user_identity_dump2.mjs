import { DatabaseSync } from 'node:sqlite';

const dbPath = process.argv[2];
const db = new DatabaseSync(dbPath, { readOnly: true });

const users = db.prepare('SELECT _id,__refid,createdAt,updatedAt,data FROM documents WHERE __s = ?').all('user_account');
console.log('=== user_account data ===');
const seen = new Map();
for (const u of users) {
  const d = JSON.parse(u.data);
  const bad = typeof d.username === 'string' && d.username !== d.username.trim().toLowerCase();
  const key = typeof d.username === 'string' ? d.username.toLowerCase() : String(d.username);
  seen.set(key, (seen.get(key) || 0) + 1);
  console.log(JSON.stringify({
    _id: u._id, refid: u.__refid, mixedCase: bad, username: d.username, cardNumber: d.cardNumber,
    admin: d.admin, discordId: d.discordId, createdAt: d.createdAt, lastLogin: d.lastLogin, keys: Object.keys(d).join(','),
  }));
}
console.log('dup lowercased usernames:', [...seen.entries()].filter(([, n]) => n > 1));
console.log('mixed-case usernames remaining:', users.map((u) => JSON.parse(u.data).username).filter((n) => typeof n === 'string' && n !== n.toLowerCase()));

console.log('\n=== card data (all) ===');
const cards = db.prepare('SELECT _id,__refid,data FROM documents WHERE __s = ?').all('card');
for (const c of cards) console.log(JSON.stringify(JSON.parse(c.data)));

console.log('\n=== all distinct __s ===');
console.log(db.prepare('SELECT __s, COUNT(*) c FROM documents GROUP BY __s ORDER BY __s').all());

console.log('\n=== api_token data ===');
for (const r of db.prepare('SELECT data FROM documents WHERE __s = ?').all('api_token')) console.log(JSON.stringify(JSON.parse(r.data)));
db.close();
