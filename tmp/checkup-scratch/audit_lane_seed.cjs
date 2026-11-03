// Insert a legacy mixed-case account into the scratch lane DB (mixed case is what
// an un-normalized / imported / arcade DB looks like).
const { DatabaseSync } = require('node:sqlite');
const bcrypt = require('C:/mfg/tmp/RyuNET-core/node_modules/bcryptjs');
const db = new DatabaseSync('C:/mfg/tmp/RyuNET-core/scratch-run/lane-id/dist/savedata/core.db');
const hash = bcrypt.hashSync('VictimPass1', 10);
const now = Date.now();
const id = 'LEGACYTEST000001';
db.prepare('INSERT INTO documents (_id,__s,__refid,createdAt,updatedAt,data) VALUES (?,?,?,?,?,?)').run(
  id, 'user_account', null, now, now,
  JSON.stringify({ username: 'LegacyUser', password: hash, cardNumber: 'A1B2C3D4E5F60718', admin: false, createdAt: now, lastLogin: now })
);
const rows = db.prepare("SELECT _id,data FROM documents WHERE __s='user_account'").all();
console.log('user_account rows now:', rows.length);
for (const r of rows) { const j = JSON.parse(r.data); console.log(r._id, JSON.stringify(j.username), j.username === j.username.toLowerCase() ? 'lower' : 'MIXED'); }
db.close();
