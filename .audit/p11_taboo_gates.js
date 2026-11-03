// p11 - Report 2: which screens the taboo matcher is applied to.
//
//   node .audit/p11_taboo_gates.js <label> [pkgDir]
//
// <label>  names the run dir (tmp/p11taboo/run/<label>)
// <pkgDir> the plugin TypeScript tree to transpile and load.
//          pre/pkg  = the build as it stands before this fix
//          dist/plugins/mfg@asphyxia = the canonical runtime copy (after)
//
// Everything below goes through the REAL handlers on a fresh savedata (in-process,
// no AsphyxiaCore, no network): /chk_tabooword, /create_player, /client_state_write,
// /get_menudata, /gchat, /gget_stamp_info. The plugin's own [VFG] log lines are
// captured and printed, because they are the evidence of WHY a write was refused.
'use strict';
const fs = require('fs');
const path = require('path');

const ROOT = path.join(__dirname, '..');
const CORE = path.join(ROOT, 'tmp', 'RyuNET-core');
const ts = require(path.join(CORE, 'node_modules', 'typescript'));

const LABEL = process.argv[2] || 'post';
const PKG = path.resolve(process.argv[3] || path.join(ROOT, 'tmp', 'p11taboo', 'pre', 'pkg'));
const RUN = path.join(ROOT, 'tmp', 'p11taboo', 'run', LABEL);
const OUT = path.join(RUN, 'dist', 'plugins', 'mfg@asphyxia');

fs.rmSync(RUN, { recursive: true, force: true });
fs.mkdirSync(OUT, { recursive: true });

// ---------------------------------------------------------------- savedata
// Same SqliteStore the core itself uses, so the plugin's DB.Find / saveProfile
// code runs unmodified against a real SQLite file in the run dir.
fs.writeFileSync(path.join(RUN, 'stores.cjs'), `
'use strict';
const path = require('path');
const { SqliteStore } = require(${JSON.stringify(path.join(CORE, 'dist', 'utils', 'SqliteStore.js'))});
module.exports = {
  pluginStore: new SqliteStore({ filename: path.join(${JSON.stringify(RUN)}, 'mfg@asphyxia.db'), timestampData: true }),
  coreStore: new SqliteStore({ filename: path.join(${JSON.stringify(RUN)}, 'core.db'), timestampData: true }),
};
`.trim(), 'utf8');

const { pluginStore } = require(path.join(RUN, 'stores.cjs'));

const logLines = [];
const out = console.log.bind(console);
console.log = (...a) => {
  const s = a.map((x) => (typeof x === 'string' ? x : String(x))).join(' ');
  if (/^\[VFG/.test(s)) { logLines.push(s); return; }
  out(...a);
};
function showLogs(pattern, label) {
  const keep = logLines.filter((l) => pattern.test(l));
  logLines.length = 0;
  if (!keep.length) return;
  out(`  -- plugin log: ${label} --`);
  for (const l of keep.slice(0, 14)) out(`     ${l}`);
}

// ---------------------------------------------------------------- core API stub
const checkQuery = (q) => {
  if (q && typeof q === 'object' && !Array.isArray(q)) {
    const o = {};
    for (const k of Object.keys(q)) { if (k === '__refid') continue; o[k] = checkQuery(q[k]); }
    return o;
  }
  return q;
};
const cleanDoc = (d) => {
  if (Array.isArray(d)) { d.forEach(cleanDoc); return d; }
  if (d && typeof d === 'object') for (const p of Object.keys(d)) if (p.startsWith('__')) delete d[p];
  return d;
};
const asph = async (kind, a, b, c) => {
  let q = null, u = null, sig = {};
  if (typeof a === 'string' && b && typeof b === 'object') { q = checkQuery(b); sig = { __s: 'plugins_profile', __refid: a }; if (kind === 'Update' || kind === 'Upsert') u = checkQuery(c); }
  else if (a == null && b && typeof b === 'object') { q = checkQuery(b); sig = { __s: 'plugins_profile' }; if (kind === 'Update' || kind === 'Upsert') u = checkQuery(c); }
  else if (a && typeof a === 'object') { q = checkQuery(a); sig = { __s: 'plugins' }; if (kind === 'Update' || kind === 'Upsert') u = checkQuery(b); }
  else throw new Error('invalid ' + kind + ' query');
  q = { ...q, ...sig };
  if (kind === 'FindOne') return cleanDoc(await pluginStore.findOneAsync(q, {}));
  if (kind === 'Find') return cleanDoc(await pluginStore.findAsync(q, {}).execAsync());
  if (kind === 'Insert') return cleanDoc(await pluginStore.insertAsync({ ...checkQuery(b || a), ...sig }));
  if (kind === 'Count') return (await pluginStore.findAsync(q, {}).execAsync()).length;
  if (kind === 'Remove') return await pluginStore.removeAsync(q, { multi: true });
  if (!Object.keys(u)[0].startsWith('$')) u = { ...u, ...sig };
  const docs = await pluginStore.updateAsync(q, u, { upsert: kind === 'Upsert', multi: true, returnUpdatedDocs: true });
  return { updated: docs.length, docs: docs.map(cleanDoc) };
};
globalThis.DB = {
  FindOne: (a, b) => asph('FindOne', a, b),
  Find: (a, b) => asph('Find', a, b),
  Insert: (a, b) => asph('Insert', a, b),
  Update: (a, b, c) => asph('Update', a, b, c),
  Upsert: (a, b, c) => asph('Upsert', a, b, c),
  Remove: (a, b) => asph('Remove', a, b),
  Count: (a, b) => asph('Count', a, b),
};
globalThis.U = { GetConfig: (key) => (key === 'VFG_VERBOSE' ? '1' : undefined) };
globalThis.K = {};
globalThis.IO = { ReadFile: () => null, Resolve: () => '' };
globalThis.CONFIG = { port: 8083 };
globalThis.R = {
  AogRoute: () => undefined, AogUnhandled: () => undefined, Config: () => undefined,
  GameCode: () => undefined, Contributor: () => undefined, Route: () => undefined,
  Unhandled: () => undefined, DataFile: () => undefined, WebUIEvent: () => undefined,
  ExtraModuleHandler: () => undefined, AogRoute_: () => undefined,
};

const shimDir = path.join(RUN, 'dist', 'utils');
fs.mkdirSync(shimDir, { recursive: true });
fs.writeFileSync(path.join(shimDir, 'EamuseIO.js'), `
const stores = require(${JSON.stringify(path.join(RUN, 'stores.cjs'))});
module.exports = {
  async FindProfile(refid) {
    const p = await stores.coreStore.findOneAsync({ __s: 'profile', __refid: refid }, {});
    if (p) return p;
    return await stores.coreStore.findOneAsync({ __s: 'refid_alias', __refid: refid }, {});
  },
  async EnsurePhantomProfile(refid, gameCode) {
    const found = await module.exports.FindProfile(refid);
    if (found) return found;
    return await stores.coreStore.insertAsync({ __s: 'refid_alias', __refid: refid, gameCode, alias: true });
  },
};
`.trim(), 'utf8');

// ---------------------------------------------------------------- transpile + load
const tsFiles = [];
(function walk(dir, rel) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    const r = rel ? `${rel}/${e.name}` : e.name;
    if (e.isDirectory()) walk(p, r);
    else if (e.name.endsWith('.ts')) tsFiles.push([p, r]);
    else fs.cpSync(p, path.join(OUT, r));
  }
})(PKG, '');
for (const [src, rel] of tsFiles) {
  const js = ts.transpileModule(fs.readFileSync(src, 'utf8'), {
    fileName: src,
    compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022 },
  }).outputText;
  const outPath = path.join(OUT, rel.replace(/\.ts$/, '.js'));
  fs.mkdirSync(path.dirname(outPath), { recursive: true });
  fs.writeFileSync(outPath, js, 'utf8');
}

// ---------------------------------------------------------------- corpora
// The compound insult forms the word-boundary rule un-caught (Report 2's list),
// the bare entries (refused by every rule), and the innocent names the rule was
// introduced to protect.
const COMPOUND = [
  'カス野郎', 'クソ野郎', 'ブス野郎', '死ねバカ', '殺すぞ', 'カスです', '売春婦', '奴隷状態',
  'fuckyou', 'youfuck', 'cuntface', 'rapes', 'slutboy', 'scamman',
];
const PLAIN = ['カス', 'クソ', '死ね', '売春', '奴隷', 'fuck', 'cunt', 'slut', 'scam', 'rape'];
const INNOCENT = ['カスミ', 'Kasumi Aoi', 'Grape', 'Grape Aoi', 'Scamp', 'Drapes', 'クスコ', 'マスカット', '松田', 'SELECTA CHARA'];

// ---------------------------------------------------------------- request harness
function ctx() {
  const res = { headersSent: false, sent: false, body: '', set() {}, send(x) { this.body = String(x); this.sent = true; this.headersSent = true; } };
  return { ctx: { req: { headers: { host: '127.0.0.1:8083' }, hostname: '127.0.0.1' }, res }, res };
}
async function call(handler, form) {
  const { ctx: c, res } = ctx();
  await handler(form, c);
  const body = res.body;
  return {
    code: (body.match(/<serv_st><code>(\d+)<\/code>/) || [])[1] ?? 'none',
    session: (body.match(/<session_id>([0-9a-f]{32})<\/session_id>/) || [])[1] || '',
    name: (body.match(/<name>([^<]*)<\/name>/) || [])[1],
    states: Object.fromEntries([...body.matchAll(/<state kind="([^"]+)"><data>([^<]*)<\/data>/g)].map((m) => [m[1], m[2]])),
    tabooResult: (body.match(/<taboo_chk><result>(\d+)<\/result>/) || [])[1],
    stamps: [...body.matchAll(/<d idx="(\d+)"[^>]*><name>([^<]*)<\/name><contents>([^<]*)<\/contents>/g)].map((m) => ({ idx: m[1], name: m[2], contents: m[3] })),
    bytes: body.length,
  };
}
const b64 = (o) => Buffer.from(JSON.stringify(o), 'utf8').toString('base64');
const unB64 = (s) => { try { return JSON.parse(Buffer.from(String(s), 'base64').toString('utf8')); } catch { return null; } };
const show = (x) => (x === undefined || x === null ? '<none>' : JSON.stringify(x));
const profileAt = async (refid) => cleanDoc(await pluginStore.findOneAsync({ __s: 'plugins_profile', __refid: refid, collection: 'profile' }, {}));

const LEGACY_REFID = 'P11LEGACY0000000001';
const LEGACY_NAME = 'rape lord';
const KASUMI_REFID = 'P11KASUMI0000000001';
const KASUMI_NAME = 'カスミ';
function seedLegacyAccount(refid, name, playerGame, cc) {
  const doc = {
    collection: 'profile', refid, name, guest: false,
    player_id: 101010101,
    states: { player_game: playerGame, cc_request: cc },
    created_at: 1730000000,
  };
  if (refid === KASUMI_REFID) doc.player_id = 202020202;
  return pluginStore.updateAsync(
    { __s: 'plugins_profile', __refid: refid, collection: 'profile' },
    { ...doc, __s: 'plugins_profile', __refid: refid },
    { upsert: true, multi: false });
}

const tally = { screenCompoundRefused: 0, screenPlainRefused: 0, gateCompoundRefused: 0, gateCompoundAccepted: [] };

(async () => {
  await seedLegacyAccount(LEGACY_REFID, LEGACY_NAME,
    JSON.stringify({ DataVersion: 14, PlayerName: LEGACY_NAME, SelectChara: 3, CharaData: [] }),
    JSON.stringify({ DataVersion: 1, now_idx: 2, done_idx: 0, m_ccInfos: [{ idx: 1, kind: 'MemorialCard', param: 'OID_12AgariSR01', name: LEGACY_NAME }] }));
  await seedLegacyAccount(KASUMI_REFID, KASUMI_NAME,
    JSON.stringify({ DataVersion: 14, PlayerName: KASUMI_NAME, SelectChara: 3, CharaData: [] }),
    JSON.stringify({ DataVersion: 1, now_idx: 2, done_idx: 0, m_ccInfos: [{ idx: 1, kind: 'MemorialCard', param: 'OID_12AgariSR01', name: 'カスミの記念カード' }] }));
  const aog = require(path.join(OUT, 'handlers', 'aog.js'));
  const utils = require(path.join(OUT, 'handlers', 'utils.js'));
  aog.registerAogRoutes();
  const narrow = utils.isTabooTextStandalone || null;

  console.log(`build : ${LABEL} (${PKG})`);
  console.log(`run   : ${RUN}`);
  console.log(`words : ${utils.TABOO_WORDS.length} entries; gate matcher: ${narrow ? 'isTabooTextStandalone' : 'none (shared matcher only)'}\n`);

  console.log('=== 0. the matcher functions themselves ===');
  console.log('  probe'.padEnd(20) + 'isTabooText (entry screens)'.padEnd(30) + 'isTabooTextStandalone (state-write gate)');
  for (const group of [['COMPOUND', COMPOUND], ['PLAIN', PLAIN], ['INNOCENT', INNOCENT]]) {
    for (const p of group[1]) {
      const a = utils.isTabooText(p);
      const b = narrow ? narrow(p) : null;
      if (group[0] === 'COMPOUND' && a) tally.screenCompoundRefused++;
      if (group[0] === 'PLAIN' && a) tally.screenPlainRefused++;
      console.log(`  ${JSON.stringify(p).padEnd(18)}${(a === null ? 'ACCEPTED' : `hit '${a}'`).padEnd(28)}${narrow ? (b === null ? 'ACCEPTED' : `hit '${b}'`) : '-'}`);
    }
  }

  console.log('\n=== 1. /chk_tabooword (the registration name screen) ===');
  for (const p of [...COMPOUND, ...PLAIN]) {
    const r = await call(aog.handle_chk_tabooword, { web_id: '1', area_id: '1', str: p });
    if (r.tabooResult !== '1') tally.gateCompoundAccepted.push(`chk_tabooword ${p}`);
    console.log(`  chk_tabooword ${JSON.stringify(p).padEnd(14)} -> result=${r.tabooResult}${r.tabooResult === '1' ? ' (name refused)' : ' (NAME ACCEPTED)'}`);
  }
  showLogs(/chk_tabooword/, 'section 1');

  console.log('\n=== 2. /create_player (the name the account is created with) ===');
  for (const [i, p] of COMPOUND.slice(0, 4).entries()) {
    const refid = `P11CREATE000000000${i}`;
    const l = await call(aog.handle_login, { web_id: '1', area_id: '1', user_id: refid, bypass_auth: 'true', guest: '1' });
    const c = await call(aog.handle_create_player, { web_id: '1', area_id: '1', pcuid: l.session, user_id: refid, name: p });
    const prof = await profileAt(refid);
    console.log(`  create_player name=${JSON.stringify(p).padEnd(12)} -> code=${c.code}; profile.name=${show(prof && prof.name)}`);
  }
  showLogs(/create_player/, 'section 2');

  console.log('\n=== 3. the cited live chain: chk_tabooword -> client_state_write -> get_menudata ===');
  for (const [i, p] of ['カス野郎', 'クソ野郎', '売春婦', 'cuntface'].entries()) {
    const refid = `P11CHAIN0000000000${i}`;
    const l = await call(aog.handle_login, { web_id: '1', area_id: '1', user_id: refid, bypass_auth: 'true', guest: '1' });
    const t = await call(aog.handle_chk_tabooword, { web_id: '1', area_id: '1', str: p });
    const w = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid: String((await profileAt(refid)).player_id), kind: 'player_game', data: b64({ DataVersion: 14, PlayerName: p, SelectChara: 0, CharaData: [] }) });
    const m = await call(aog.handle_get_menudata, { web_id: '1', area_id: '1', pcuid: l.session });
    if (w.code === '0') tally.gateCompoundAccepted.push(`state_write ${p}`);
    console.log(`  ${JSON.stringify(p).padEnd(10)} chk_tabooword=${t.tabooResult} state_write=${w.code} get_menudata name=${show(m.name)}`);
  }
  showLogs(/chk_tabooword|client_state_write|mirror/, 'section 3');

  console.log('\n=== 4. every compound form through the state-write gate ===');
  {
    const refid = 'P11GATE00000000000001';
    const l = await call(aog.handle_login, { web_id: '1', area_id: '1', user_id: refid, bypass_auth: 'true', guest: '1' });
    const mid = String((await profileAt(refid)).player_id);
    for (const p of COMPOUND) {
      const w = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'player_game', data: b64({ DataVersion: 14, PlayerName: p, SelectChara: 0, CharaData: [] }) });
      if (w.code === '0') { tally.gateCompoundAccepted.push(`state_write ${p}`); tally.gateCompoundRefused--; }
      tally.gateCompoundRefused++;
      console.log(`  PlayerName=${JSON.stringify(p).padEnd(12)} -> code=${w.code}${w.code === '0' ? ' (LANDED)' : ' (refused)'}`);
    }
    const served = unB64((await call(aog.handle_client_state_read, { web_id: '1', area_id: '1', pcuid: l.session, mid })).states.player_game);
    console.log(`  account still serving PlayerName=${show(served && served.PlayerName)}`);
  }
  showLogs(/client_state_write/, 'section 4');

  console.log('\n=== 5. /gchat and /gget_stamp_info (table chat, every seat sees it) ===');
  {
    const refid = 'P11CHAT0000000000001';
    const l = await call(aog.handle_login, { web_id: '1', area_id: '1', user_id: refid, bypass_auth: 'true', guest: '1' });
    const mid = String((await profileAt(refid)).player_id);
    const tid = 9101;
    const g1 = await call(aog.handle_gchat, { web_id: '1', area_id: '1', pcuid: l.session, tid, mid, pindex: '0', name: 'カス野郎', contents: 'TableSticker001', param: '' });
    console.log(`  gchat name='カス野郎' contents='TableSticker001' -> stamps: ${JSON.stringify(g1.stamps.filter((s) => Number(s.mid) !== 0))}`);
    const g2 = await call(aog.handle_gchat, { web_id: '1', area_id: '1', pcuid: l.session, tid, mid, pindex: '0', name: '松田', contents: '死ねバカ', param: '' });
    console.log(`  gchat name='松田' contents='死ねバカ'            -> stamps: ${JSON.stringify(g2.stamps.filter((s) => Number(s.mid) !== 0))}`);
    const g3 = await call(aog.handle_gchat, { web_id: '1', area_id: '1', pcuid: l.session, tid, mid, pindex: '0', name: 'カス野郎', contents: '殺すぞ', param: '' });
    console.log(`  gchat slur name + slur contents                -> stamps: ${JSON.stringify(g3.stamps.filter((s) => Number(s.mid) !== 0))}`);
    const g4 = await call(aog.handle_gchat, { web_id: '1', area_id: '1', pcuid: l.session, tid, mid, pindex: '0', name: '松田', contents: 'TableSticker002', param: '' });
    console.log(`  gchat clean name + clean contents              -> stamps: ${JSON.stringify(g4.stamps.filter((s) => Number(s.mid) !== 0))}`);
    const s1 = await call(aog.handle_gget_stamp_info, { web_id: '1', area_id: '1', pcuid: l.session, must: `1,1,${tid},0,${mid}`, stamp_info: `1,0,奴隷状態,松田` });
    console.log(`  gget_stamp_info contents='奴隷状態'            -> stamps: ${JSON.stringify(s1.stamps.filter((s) => Number(s.mid) !== 0))}`);
    const s2 = await call(aog.handle_gget_stamp_info, { web_id: '1', area_id: '1', pcuid: l.session, must: `1,1,${tid},0,${mid}`, stamp_info: `1,0,TableSticker003,カス野郎` });
    console.log(`  gget_stamp_info name='カス野郎'                -> stamps: ${JSON.stringify(s2.stamps.filter((s) => Number(s.mid) !== 0))}`);
  }
  showLogs(/gchat|stamp/, 'section 5');

  console.log('\n=== 6. no regression: the stored-name echo the narrowing was for ===');
  {
    // A fresh account cannot pick a name that merely CONTAINS a short entry - the
    // entry screen refuses it, /chk_tabooword included. Documented cost of substring
    // screening at the door new text comes through.
    const fresh = 'P11NEWNAME0000000001';
    const l = await call(aog.handle_login, { web_id: '1', area_id: '1', user_id: fresh, bypass_auth: 'true', guest: '1' });
    const p1 = await profileAt(fresh);
    const mid = String(p1.player_id);
    const w1 = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'player_game', data: b64({ DataVersion: 14, PlayerName: 'カスミ', SelectChara: 0, CharaData: [] }) });
    const w1b = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'player_game', data: b64({ DataVersion: 14, PlayerName: '松田', SelectChara: 0, CharaData: [] }) });
    console.log(`  fresh account posts a NEW name 'カスミ' -> code=${w1.code} (entry screen refuses it, same as /chk_tabooword)`);
    console.log(`  fresh account posts '松田'             -> code=${w1b.code}`);
  }
  {
    // The account the narrowing exists for: it already owns the name (seeded below,
    // like a profile created before カス was in the list) and every Save posts back
    // the document this server served. It must keep persisting player_game.
    const l = await call(aog.handle_login, { web_id: '1', area_id: '1', user_id: KASUMI_REFID, bypass_auth: 'true', guest: '1' });
    const p = await profileAt(KASUMI_REFID);
    const mid = String(p.player_id);
    const r = await call(aog.handle_client_state_read, { web_id: '1', area_id: '1', pcuid: l.session, mid });
    const servedPg = unB64(r.states.player_game);
    const echo = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'player_game', data: b64({ ...servedPg, SelectChara: 4 }) });
    const after = unB64((await call(aog.handle_client_state_read, { web_id: '1', area_id: '1', pcuid: l.session, mid })).states.player_game);
    const menu = await call(aog.handle_get_menudata, { web_id: '1', area_id: '1', pcuid: l.session });
    console.log(`  seeded ${KASUMI_REFID} stored name '${KASUMI_NAME}'`);
    console.log(`  save the served document back, SelectChara 3->4 -> code=${echo.code}; served PlayerName=${show(after && after.PlayerName)} SelectChara=${show(after && after.SelectChara)} (4 = the account can still persist)`);
    console.log(`  get_menudata name=${show(menu.name)}`);
    const slur = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'player_game', data: b64({ ...servedPg, PlayerName: 'カス野郎', SelectChara: 5 }) });
    const clean = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'player_game', data: b64({ ...servedPg, PlayerName: '松田', SelectChara: 6 }) });
    const after2 = unB64((await call(aog.handle_client_state_read, { web_id: '1', area_id: '1', pcuid: l.session, mid })).states.player_game);
    console.log(`  same account posts a NEW name 'カス野郎' -> code=${slur.code} (must stay 803)`);
    console.log(`  same account posts a NEW clean name '松田' -> code=${clean.code}; served PlayerName=${show(after2 && after2.PlayerName)}`);
  }
  {
    const l = await call(aog.handle_login, { web_id: '1', area_id: '1', user_id: LEGACY_REFID, bypass_auth: 'true', guest: '1' });
    const p = await profileAt(LEGACY_REFID);
    const mid = String(p.player_id);
    const r = await call(aog.handle_client_state_read, { web_id: '1', area_id: '1', pcuid: l.session, mid });
    const servedPg = unB64(r.states.player_game);
    const echo = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'player_game', data: b64({ ...servedPg, SelectChara: 7 }) });
    const after = unB64((await call(aog.handle_client_state_read, { web_id: '1', area_id: '1', pcuid: l.session, mid })).states.player_game);
    console.log(`  legacy name '${LEGACY_NAME}' echo save -> code=${echo.code}; served SelectChara=${show(after && after.SelectChara)} (7 = echo accepted)`);
    const cc = unB64(r.states.cc_request) || { DataVersion: 1, now_idx: 2, done_idx: 0, m_ccInfos: [] };
    const ccInnocent = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'cc_request', data: b64({ ...cc, m_ccInfos: [...cc.m_ccInfos, { idx: 5, kind: 'Friend', param: 'P11OTHER00000000005', name: 'カスミ' }] }) });
    const ccSlur = await call(aog.handle_client_state_write, { web_id: '1', area_id: '1', pcuid: l.session, mid, kind: 'cc_request', data: b64({ ...cc, m_ccInfos: [...cc.m_ccInfos, { idx: 6, kind: 'Friend', param: 'P11OTHER00000000006', name: 'カス野郎' }] }) });
    console.log(`  cc_request row named 'カスミ' (innocent) -> code=${ccInnocent.code} (must be 0: a card row cannot be renamed by the client)`);
    console.log(`  cc_request row named 'カス野郎'          -> code=${ccSlur.code} (must stay 803)`);
  }
  showLogs(/taboo|echo/, 'section 6');

  console.log('\n=== 7. what the substring screens now do with innocent names ===');
  for (const p of INNOCENT) {
    const r = await call(aog.handle_chk_tabooword, { web_id: '1', area_id: '1', str: p });
    console.log(`  chk_tabooword ${JSON.stringify(p).padEnd(16)} -> result=${r.tabooResult}${r.tabooResult === '1' ? ' (name refused at registration)' : ''}`);
  }
  showLogs(/chk_tabooword/, 'section 7');

  console.log('\n=== verdict ===');
  console.log(`  compound forms refused by isTabooText (entry screens): ${tally.screenCompoundRefused}/${COMPOUND.length}`);
  console.log(`  plain entries refused by isTabooText                 : ${tally.screenPlainRefused}/${PLAIN.length}`);
  console.log(`  compound forms refused at chk_tabooword + state write: ${COMPOUND.length - tally.gateCompoundAccepted.filter((x) => x.startsWith('chk_tabooword')).length}/${COMPOUND.length} chk, ${tally.gateCompoundRefused}/${COMPOUND.length} gate`);
  console.log(`  accepted compound forms (the hole): ${tally.gateCompoundAccepted.length ? tally.gateCompoundAccepted.join(', ') : 'none'}`);
})().catch((e) => { console.error('HARNESS FAILED:', e && e.stack ? e.stack : e); process.exit(1); });
