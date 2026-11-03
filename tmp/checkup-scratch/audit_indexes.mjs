import { DatabaseSync } from 'node:sqlite';
const db = new DatabaseSync('C:/mfg/tmp/RyuNET-core/dist/savedata/core.db', { readOnly: true });
console.log(db.prepare("SELECT name,`unique`,origin FROM pragma_index_list('documents')").all());
console.log(db.prepare("SELECT name FROM pragma_table_info('documents') WHERE `notnull`=1 OR pk>0").all());
db.close();
