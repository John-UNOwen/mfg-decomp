// Read-only probe of the live savedata: identity docs written by signup mint + plugin profiles.
const { DatabaseSync } = require('node:sqlite');
const path = require('path');

const SD = path.join(__dirname, '..', 'tmp', 'RyuNET-core', 'dist', 'savedata');
const out = [];
const say = (s) => out.push(s);

function open(file) {
  return new DatabaseSync(file, { readOnly: true });
}

function tables(db) {
  return db.prepare("SELECT name, sql FROM sqlite_master WHERE type='table'").all();
}
function indexes(db) {
  const rows = db.prepare("SELECT name, sql FROM sqlite_master WHERE type='index'").all();
  const info = db.prepare("PRAGMA index_list('documents')").all();
  const uniq = new Map(info.map(i => [String(i.name), !!i.unique]));
  return rows.map(r => ({ name: r.name, u: uniq.has(r.name) ? uniq.get(r.name) : /UNIQUE/i.test(r.sql || ''), sql: r.sql }));
}

function probe(label, file) {
  say('==================================================');
  say('## ' + label + '  ' + file);
  let db;
  try { db = open(file); } catch (e) { say('OPEN FAILED: ' + e.message); return null; }
  say('tables: ' + tables(db).map(t => t.name).join(', '));
  const idx = indexes(db);
  say('indexes: ' + idx.map(i => i.name + (i.u ? ' [UNIQUE]' : '')).join(', '));
  const anyUnique = idx.some(i => i.u);
  say('any UNIQUE index: ' + anyUnique);
  return db;
}

const core = probe('core.db', path.join(SD, 'core.db'));
if (core) {
  const cols = core.prepare("SELECT name FROM pragma_table_info('documents')").all().map(c => c.name);
  say('documents columns: ' + cols.join(', '));
  const groups = core.prepare("SELECT __s, COUNT(*) c FROM documents GROUP BY __s ORDER BY c DESC").all();
  say('__s counts: ' + groups.map(g => (g.__s || '(null)') + '=' + g.c).join(', '));

  const users = core.prepare("SELECT _id, __refid, data FROM documents WHERE __s='user_account'").all();
  say('--- user_account (' + users.length + ') ---');
  const byName = new Map();
  const byCard = new Map();
  for (const u of users) {
    const d = JSON.parse(u.data);
    const name = d.username, card = d.cardNumber;
    say('  username=' + JSON.stringify(name) + ' cardNumber=' + JSON.stringify(card) + ' admin=' + d.admin + ' created=' + d.createdAt);
    if (!byName.has(name)) byName.set(name, []);
    byName.get(name).push(d);
    if (card) {
      if (!byCard.has(String(card).toUpperCase())) byCard.set(String(card).toUpperCase(), []);
      byCard.get(String(card).toUpperCase()).push(name);
    }
  }
  say('duplicate usernames: ' + ([...byName.entries()].filter(([, v]) => v.length > 1).map(([k, v]) => k + ' x' + v.length).join(', ') || 'none'));
  say('usernames differing only by case: ' + (function () {
    const lower = new Map();
    for (const k of byName.keys()) { const l = String(k).toLowerCase(); if (!lower.has(l)) lower.set(l, []); lower.get(l).push(k); }
    return [...lower.entries()].filter(([, v]) => v.length > 1).map(([l, v]) => l + ': ' + v.join(' | ')).join('; ') || 'none';
  })());
  say('cardNumbers shared by >1 user: ' + ([...byCard.entries()].filter(([, v]) => v.length > 1).map(([k, v]) => k + ' -> ' + v.join(',')).join('; ') || 'none'));

  const cards = core.prepare("SELECT _id, __refid, data FROM documents WHERE __s='card'").all();
  say('--- card docs (' + cards.length + ') ---');
  const cardCids = new Map();
  for (const c of cards) {
    const d = JSON.parse(c.data);
    cardCids.set(String(d.cid).toUpperCase(), (cardCids.get(String(d.cid).toUpperCase()) || 0) + 1);
    say('  cid=' + d.cid + ' __refid=' + d.__refid + ' cid==refid=' + (String(d.cid).toUpperCase() === String(d.__refid).toUpperCase()) + ' print=' + JSON.stringify(d.print));
  }
  say('duplicate card cid: ' + [...cardCids.entries()].filter(([, n]) => n > 1).map(([k, n]) => k + ' x' + n).join(', ') || 'none');
  const shapes = {};
  for (const cid of cardCids.keys()) {
    let shape = 'other';
    if (/^[0-9A-F]{16}$/.test(cid)) shape = '16hex';
    if (/^01[0-9A-F]{14}$/.test(cid)) shape = '01+14hex';
    if (/^E004[0-9A-F]{12}$/.test(cid)) shape = 'E004+12hex';
    if (/^MID/.test(cid)) shape = 'MID';
    shapes[shape] = (shapes[shape] || 0) + 1;
  }
  say('card cid shapes: ' + JSON.stringify(shapes));

  const profiles = core.prepare("SELECT __refid, data FROM documents WHERE __s IN ('profile','refid_alias')").all();
  say('--- core profile/refid_alias (' + profiles.length + ') ---');
  const coreRefids = new Set();
  const aliasList = [];
  for (const p of profiles) {
    const d = JSON.parse(p.data);
    const r = String(d.__refid || p.__refid || '').toUpperCase();
    coreRefids.add(r);
    if (d.alias) aliasList.push(r);
  }
  say('core refids: ' + [...coreRefids].join(','));
  say('core refid_alias docs: ' + (aliasList.join(',') || 'none'));
  const mintSpace = [...coreRefids].filter(r => /^[0-9A-F]{16}$/.test(r));
  say('core refids inside the 16-hex mint space: ' + mintSpace.length + ' -> ' + mintSpace.join(','));
  global.__coreRefids = coreRefids;
}

const plug = probe('mfg@asphyxia.db', path.join(SD, 'mfg@asphyxia.db'));
if (plug) {
  const groups = plug.prepare("SELECT __s, _collection, COUNT(*) c FROM documents GROUP BY __s, _collection ORDER BY c DESC").all();
  say('counts: ' + groups.map(g => (g.__s || '(null)') + '/' + (g._collection || '(null)') + '=' + g.c).join(', '));
  const profs = plug.prepare("SELECT _id, __refid, data FROM documents WHERE _collection='profile'").all();
  say('--- plugin profiles (' + profs.length + ') ---');
  const missing = [];
  const hexOnes = [];
  for (const p of profs) {
    const d = JSON.parse(p.data);
    const r = String(d.refid || p.__refid || '').toUpperCase();
    if (/^[0-9A-F]{16}$/.test(r)) hexOnes.push(r);
    if (!global.__coreRefids.has(r)) missing.push(r + ' (card_id=' + JSON.stringify(d.card_id || '') + ')');
    say('  refid=' + r + ' name=' + JSON.stringify(d.name) + ' card_id=' + JSON.stringify(d.card_id || '') + ' card_issued=' + JSON.stringify(d.card_issued || ''));
  }
  say('plugin profiles with NO core profile/refid_alias: ' + (missing.length ? missing.join('; ') : 'none'));
  say('plugin profile refids in the 16-hex mint space: ' + hexOnes.length + ' -> ' + hexOnes.join(','));
}

console.log(out.join('\n'));
