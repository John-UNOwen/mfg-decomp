import { DatabaseSync } from 'node:sqlite';
const core = new DatabaseSync('C:/mfg/tmp/RyuNET-core/dist/savedata/core.db', { readOnly: true });
const users = core.prepare("SELECT data FROM documents WHERE __s='user_account'").all().map((r) => JSON.parse(r.data));
const names = new Set(users.map((u) => String(u.username).toLowerCase()));
const cardOf = new Map(users.map((u) => [u.cardNumber, u.username]));
core.close();

const db = new DatabaseSync('C:/mfg/tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db', { readOnly: true });
const profs = db.prepare("SELECT _id,__refid,data FROM documents WHERE __s='plugins_profile'").all();
let nameMatches = 0;
const sample = [];
for (const p of profs) {
  let j; try { j = JSON.parse(p.data); } catch { continue; }
  const n = String(j.name ?? '');
  if (names.has(n.toLowerCase())) { nameMatches++; sample.push({ refid: p.__refid, name: n, card_id: j.card_id, bound: j.card_issued }); }
  if (sample.length < 6 && p.__refid && cardOf.has(String(p.__refid).toUpperCase()))
    sample.push({ refid: p.__refid, name: n, card_id: j.card_id, ownerUser: cardOf.get(String(p.__refid).toUpperCase()) });
}
console.log('plugin profiles:', profs.length);
console.log('profiles whose name matches a web username:', nameMatches);
console.log(sample.slice(0, 12));
const refids = profs.map((p) => p.__refid);
const dupRefids = (() => { const m = new Map(); for (const r of refids) m.set(r, (m.get(r) || 0) + 1); return [...m].filter(([, c]) => c > 1); })();
console.log('duplicate plugin profile refids:', dupRefids);
console.log('plugin profiles keyed by a web-issued DataID:', refids.filter((r) => cardOf.has(String(r).toUpperCase())).length);
const others = db.prepare("SELECT _id,__refid,data FROM documents WHERE __s='plugins'").all();
console.log('plugins docs:', others.length, others.slice(0, 3).map((o) => o.__refid + ' / ' + String(o.data).slice(0, 80)));
db.close();
