import { DatabaseSync } from 'node:sqlite';
const db = new DatabaseSync('C:/mfg/tmp/RyuNET-core/dist/savedata/core.db', { readOnly: true });
const names = db.prepare("SELECT data FROM documents WHERE __s='user_account'").all().map(r => JSON.parse(r.data).username);
console.log('live user count:', names.length);
console.log('audit probe names present in live DB:', names.filter(n => ['ClaimUser','claimuser','LegacyUser','legacyuser','raceuser','zetauser','ZetaUser','WobbleUser','wobbleuser'].includes(n)));
console.log('cards in live db:', db.prepare("SELECT COUNT(*) c FROM documents WHERE __s='card'").get().c);
db.close();
