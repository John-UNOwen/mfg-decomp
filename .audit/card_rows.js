const { DatabaseSync } = require('node:sqlite');
const path = require('path');
const db = new DatabaseSync(path.join(process.cwd(), 'tmp', 'RyuNET-core', 'dist', 'savedata', 'core.db'), { readOnly: true });
const rows = db.prepare("SELECT _id, __s, __refid, createdAt, updatedAt, data FROM documents WHERE __s='card'").all();
for (const r of rows) console.log(JSON.stringify({ _id: r._id, col_refid: r.__refid, createdAt: r.createdAt, updatedAt: r.updatedAt, data: r.data }));
const ua = db.prepare("SELECT _id, __refid, createdAt, data FROM documents WHERE __s='user_account'").all();
for (const r of ua) console.log('USER col_refid=' + JSON.stringify(r.__refid) + ' created=' + r.createdAt + ' data=' + r.data.slice(0, 220));
db.close();
