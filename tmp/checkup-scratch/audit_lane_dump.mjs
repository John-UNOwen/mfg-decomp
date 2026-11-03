import { DatabaseSync } from 'node:sqlite';
const db = new DatabaseSync('C:/mfg/tmp/RyuNET-core/scratch-run/lane-id/dist/savedata/core.db', { readOnly: true });
for (const r of db.prepare("SELECT _id,data FROM documents WHERE __s='user_account'").all()) {
  const j = JSON.parse(r.data);
  console.log(JSON.stringify({ id: r._id, username: j.username, card: j.cardNumber }));
}
console.log('cards:', db.prepare("SELECT COUNT(*) c FROM documents WHERE __s='card'").get().c);
db.close();
