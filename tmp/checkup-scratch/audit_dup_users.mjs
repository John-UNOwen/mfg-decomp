import { DatabaseSync } from 'node:sqlite';
const files = ['savedata12', 'savedata2', 'savedataATK4', 'savedataATK1', 'savedata8'];
for (const d of files) {
  const f = 'C:/mfg/tmp/RyuNET-core/scratch-run/' + d + '/core.db';
  const db = new DatabaseSync(f, { readOnly: true });
  const rows = db.prepare("SELECT _id,createdAt,data FROM documents WHERE __s='user_account' ORDER BY createdAt").all();
  console.log('=== ' + d + ' ===');
  for (const r of rows) {
    const j = JSON.parse(r.data);
    console.log(JSON.stringify({ username: j.username, card: j.cardNumber, createdAt: j.createdAt, id: r._id }));
  }
  const cards = db.prepare("SELECT COUNT(*) c FROM documents WHERE __s='card'").get().c;
  console.log('cards:', cards);
  db.close();
}
