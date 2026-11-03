import { DatabaseSync } from 'node:sqlite';
const db = new DatabaseSync('C:/mfg/tmp/RyuNET-core/dist/savedata/core.db', { readOnly: true });
const users = db.prepare("SELECT _id,data FROM documents WHERE __s='user_account'").all().map((r) => ({ id: r._id, ...JSON.parse(r.data) }));
const cards = db.prepare("SELECT _id,__refid,data FROM documents WHERE __s='card'").all().map((r) => ({ id: r._id, refid: r.__refid, ...JSON.parse(r.data) }));
const aliases = db.prepare("SELECT __refid FROM documents WHERE __s='refid_alias'").all().map((r) => r.__refid);

const cardByCid = new Map(cards.map((c) => [c.cid, c]));
console.log('users:', users.length, 'cards:', cards.length, 'aliases:', aliases.length);

const noCard = users.filter((u) => u.cardNumber && !cardByCid.has(u.cardNumber));
console.log('users WITH cardNumber but NO card document:', noCard.map((u) => u.username + '/' + u.cardNumber));
const adminNoCard = users.filter((u) => !u.cardNumber);
console.log('users with empty cardNumber:', adminNoCard.map((u) => u.username + '(admin=' + u.admin + ')'));

const owned = new Set(users.map((u) => u.cardNumber).filter(Boolean));
const orphanCards = cards.filter((c) => !owned.has(c.cid));
console.log('card documents no user answers to:', orphanCards.map((c) => c.cid + '->' + c.refid));

const badRefid = cards.filter((c) => c.refid !== c.cid);
console.log('cards where __refid != cid:', badRefid.map((c) => c.cid + '->' + c.refid));

// duplicate cardNumbers across users (two identities sharing one DataID)
const byNum = new Map();
for (const u of users) if (u.cardNumber) byNum.set(u.cardNumber, (byNum.get(u.cardNumber) || 0) + 1);
console.log('cardNumbers shared by >1 user:', [...byNum].filter(([, n]) => n > 1));

// duplicate alias refids
const aliasCount = new Map();
for (const a of aliases) aliasCount.set(a, (aliasCount.get(a) || 0) + 1);
console.log('duplicate refid_alias refids:', [...aliasCount].filter(([, n]) => n > 1));

// users whose minted DataID also has a refid alias (played at least once)
console.log('users with a refid alias for their DataID:', users.filter((u) => u.cardNumber && aliasCount.has(u.cardNumber)).length);
db.close();
