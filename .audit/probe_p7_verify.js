// P7 verification: dojo session handling, friend_game served as saved, odekake borrow.
// Runs against a live core (canonical tree: cwd <repo>/tmp/RyuNET-core/dist,
// node AsphyxiaCore.js; port from argv[2]) or a staged lane. The DB default is the
// canonical dist savedata, named in full instead of "whatever directory this file
// happens to be run from".
// Account fixtures are freshly-suffixed per run: friend lists, borrow grants, music
// gacha ownership and notice acks are permanent per-account server state, so a pinned
// account measures yesterday's fixture instead of this run.
const BASE = (process.argv[2] || "http://127.0.0.1:8083") + "/aog/";
const DBPATH = process.argv[3] || "C:/mfg/tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db";
const SUF = String(Date.now()).slice(-5);
const STARTED_AT = Date.now();
const RL = 'P7LENDER' + SUF;
const RB = 'P7BORROW' + SUF;
const RG = 'P7GHOST' + SUF;

let fails = 0;
function check(name, ok, detail) {
  console.log(`${ok ? "PASS" : "FAIL"}  ${name}${detail ? "  | " + detail : ""}`);
  if (!ok) fails++;
}
async function post(name, body) {
  const res = await fetch(BASE + name, {
    method: "POST",
    headers: { "content-type": "application/x-www-form-urlencoded", "user-agent": "curl/8" },
    body: new URLSearchParams(body).toString(),
  });
  const text = await res.text();
  const code = Number((text.match(/<serv_st><code>(\d+)<\/code>/) || [])[1] ?? "NaN");
  return { status: res.status, text, code };
}
function b64json(s) { return Buffer.from(s, "utf-8").toString("base64"); }
function statesOf(text) {
  const out = {};
  const re = /<state kind="([^"]+)"><data>([^<]*)<\/data><\/state>/g;
  let m;
  while ((m = re.exec(text))) {
    try { out[m[1]] = Buffer.from(m[2], "utf-8").toString("base64") === "" ? "" : JSON.parse(Buffer.from(m[2], "base64").toString("utf-8")); }
    catch { out[m[1]] = null; }
  }
  return out;
}

const LENDER_PG = JSON.stringify({
  DataVersion: 14, PlayerID: 0, PlayerName: "LenderChan", SelectChara: 3,
  m_danLevel: 6, m_dojoSlotUnlocked: [true, false, false, false],
  CharaData: Array.from({ length: 21 }, (_, i) => ({ charaType: i, Level: 1, Exp: 0, IsDebut: i === 3, CostumeItemEquip: 1000 + i })),
});
const LENDER_CUST = JSON.stringify({ DataVersion: 1, CustomizeItems: [] });

(async () => {
  console.log("target:", BASE);

  // ---- A. dojo with no account behind the session ----
  const bogus = "0000000000000000000000000000dead";
  const d1 = await post("dojo_get_status", { web_id: 1, area_id: 1, pcuid: bogus });
  check("A1 dojo_get_status(bogus pcuid) -> serv_st 770", d1.code === 770 && !d1.text.includes("<dojo>"), `code=${d1.code} body=${d1.text.slice(0, 120)}`);
  const d2 = await post("dojo_set_slot", { web_id: 1, area_id: 1, pcuid: bogus, slot_id: 0, set_character: "OID_CHARACTER_7" });
  check("A2 dojo_set_slot(bogus pcuid) -> serv_st 770", d2.code === 770, `code=${d2.code}`);
  const d3 = await post("dojo_gain_soul", { web_id: 1, area_id: 1, pcuid: bogus, slot_id: 0 });
  check("A3 dojo_gain_soul(bogus pcuid) -> serv_st 770", d3.code === 770, `code=${d3.code}`);

  // ---- B. two real accounts ----
  const loginL = await post("login", { web_id: 1, area_id: 1, user_id: RL });
  const loginB = await post("login", { web_id: 1, area_id: 1, user_id: RB });
  const pcL = (loginL.text.match(/<session_id>([0-9a-f]+)<\/session_id>/) || [])[1];
  const pcB = (loginB.text.match(/<session_id>([0-9a-f]+)<\/session_id>/) || [])[1];
  check("B0 login mints sessions", !!pcL && !!pcB, `lender=${pcL} borrower=${pcB}`);
  await post("create_player", { web_id: 1, area_id: 1, pcuid: pcL, user_id: RL, name: "LenderChan" });
  await post("create_player", { web_id: 1, area_id: 1, pcuid: pcB, user_id: RB, name: "BorrowerChan" });

  const midL = Number((await post("get_menudata", { web_id: 1, area_id: 1, pcuid: pcL })).text.match(/<mid>(\d+)<\/mid>/)?.[1]);
  const midB = Number((await post("get_menudata", { web_id: 1, area_id: 1, pcuid: pcB })).text.match(/<mid>(\d+)<\/mid>/)?.[1]);
  check("B1 both accounts have their own mid", midL && midB && midL !== midB, `midL=${midL} midB=${midB}`);

  const setL = await post("dojo_set_slot", { web_id: 1, area_id: 1, pcuid: pcL, slot_id: 0, set_character: "OID_CHARACTER_7" });
  check("B2 lender dojo_set_slot succeeds", setL.code === 0 && /<slot idx="0"><available>1/.test(setL.text), setL.text.slice(0, 160));

  const getB = await post("dojo_get_status", { web_id: 1, area_id: 1, pcuid: pcB });
  const slotsB = (getB.text.match(/<slot idx="\d">/g) || []).length;
  check("B3 borrower has its own empty dojo (4 slots, none available)",
    getB.code === 0 && slotsB === 4 && !getB.text.includes("<available>1"), `slots=${slotsB}`);
  const getL = await post("dojo_get_status", { web_id: 1, area_id: 1, pcuid: pcL });
  check("B4 lender keeps its own lesson", /<slot idx="0"><available>1<\/available><character_obj>OID_CHARACTER_7/.test(getL.text), getL.text.match(/<slot idx="0">[^<]*(?:<[^>]+>[^<]*)*/)?.[0]?.slice(0, 160));

  // stale session after relogin must not read the account's dojo either
  // card-less accounts rotate on login (last login wins, takeover_grace 0): the evicted
  // pcuid is tombstoned and answers 770, the new session carries the account.
  await new Promise((r) => setTimeout(r, 2000));
  const reloginL = await post("login", { web_id: 1, area_id: 1, user_id: RL });
  const pcL2 = (reloginL.text.match(/<session_id>([0-9a-f]+)<\/session_id>/) || [])[1];
  const stale = await post("dojo_get_status", { web_id: 1, area_id: 1, pcuid: pcL });
  const fresh = await post("dojo_get_status", { web_id: 1, area_id: 1, pcuid: pcL2 });
  check("B5 superseded session is refused, new session keeps the dojo",
    stale.code === 770 && fresh.code === 0 && /OID_CHARACTER_7/.test(fresh.text),
    `stale=${stale.code} fresh=${fresh.code}`);

  // ---- C. friend_game served as saved ----
  await post("client_state_write", { web_id: 1, area_id: 1, pcuid: pcL2, mid: midL, kind: "player_game", data: b64json(LENDER_PG) });
  await post("client_state_write", { web_id: 1, area_id: 1, pcuid: pcL2, mid: midL, kind: "customize_item", data: b64json(LENDER_CUST) });

  const read1 = statesOf((await post("client_state_read", { web_id: 1, area_id: 1, pcuid: pcB, mid: midB, one_kind: "friend_game" })).text);
  const seeded = read1.friend_game;
  const lenderRow = (seeded?.FriendInfos || []).find((r) => r && r.PlayerName === "LenderChan" && r.Dan === 6);
  check("C1 the seeded friend list carries the lender as a usable row with a borrow grant",
    !!lenderRow && lenderRow.IsUsable === true && Number(lenderRow.OdekakeDataID) > 0 && lenderRow.CharaType === 3,
    JSON.stringify(lenderRow || seeded).slice(0, 220));

  // friend_game is a server-owned kind: the shipping client never writes it
  // (FriendGameData.Save has no caller), and a forged row must not smuggle a borrow
  // target onto the account, so the write is refused and the seed stands.
  const forged = JSON.stringify({
    DataVersion: 1,
    FriendInfos: [
      { OdekakeDataID: 999999, FriendID: RG, PlayerName: "GhostChan", Dan: 2, IsUsable: true, CharaType: 5 },
    ],
  });
  const w = await post("client_state_write", { web_id: 1, area_id: 1, pcuid: pcB, mid: midB, kind: "friend_game", data: b64json(forged) });
  check("C2 client friend_game write refused as a server-owned kind", w.code === 803, `code=${w.code}`);
  const read2 = statesOf((await post("client_state_read", { web_id: 1, area_id: 1, pcuid: pcB, mid: midB, one_kind: "friend_game" })).text);
  const rows = read2.friend_game?.FriendInfos || [];
  const row2 = rows.find((r) => r && r.PlayerName === "LenderChan" && r.Dan === 6) || {};
  check("C3 the lender row and its grant survive the next read (stable ticket, no churn)",
    !!lenderRow && row2.OdekakeDataID === lenderRow.OdekakeDataID && row2.IsUsable === true, `ticket=${row2.OdekakeDataID}`);
  check("C4 every row is servable: published accounts only, no ghost row, no self-row",
    rows.length > 0 && rows.every((r) => r.IsUsable === true && Number(r.OdekakeDataID) > 0) &&
    !rows.some((r) => /GhostChan|BorrowerChan/.test(String(r.PlayerName || ""))),
    rows.map((r) => `${r.PlayerName}=${r.IsUsable}`).join(" ").slice(0, 200));
  const read3 = statesOf((await post("client_state_read", { web_id: 1, area_id: 1, pcuid: pcB, mid: midB })).text);
  check("C5 full read serves the stored document too", JSON.stringify(read3.friend_game?.FriendInfos) === JSON.stringify(rows),
    `kinds=${Object.keys(read3).sort().join(",")}`);

  // ---- D. odekake borrow against the grant the read issued ----
  const ticket = Number(lenderRow?.OdekakeDataID || 0);
  const od = await post("odekake_done", { web_id: 1, area_id: 1, pcuid: pcB, odekake_id: ticket });
  const odStates = statesOf(od.text);
  const lenderChara = odStates.player_game?.CharaData?.[odStates.player_game?.SelectChara];
  check("D1 valid borrow -> code 0 + success 1 + both states",
    od.code === 0 && /<success>1<\/success>/.test(od.text) && !!odStates.customize_item && !!odStates.player_game,
    od.text.slice(0, 120));
  check("D2 served states are the lender's and carry the chara data the client indexes",
    odStates.player_game?.PlayerName === "LenderChan" && !!lenderChara,
    `PlayerName=${odStates.player_game?.PlayerName} SelectChara=${odStates.player_game?.SelectChara} chara=${JSON.stringify(lenderChara)}`);
  check("D3 id echoes the grant ticket the friend row carries", (od.text.match(/<id>([^<]*)<\/id>/)?.[1] || "") === String(ticket));

  const odGhost = await post("odekake_done", { web_id: 1, area_id: 1, pcuid: pcB, odekake_id: 999999 });
  check("D4 unknown lender -> code 0 + success 0 (never an error code)",
    odGhost.code === 0 && /<success>0<\/success>/.test(odGhost.text), `code=${odGhost.code} body=${odGhost.text.slice(0, 140)}`);
  const odNoAcct = await post("odekake_done", { web_id: 1, area_id: 1, pcuid: bogus, odekake_id: ticket });
  check("D5 borrow with no account behind the session -> success 0, code 0",
    odNoAcct.code === 0 && /<success>0<\/success>/.test(odNoAcct.text), `code=${odNoAcct.code}`);
  // The client's own half of the flow (MyRoomFlow.cs:191-197 then GotoNextScene
  // saves at :222-223): the borrower writes back its account carrying the borrow.
  const borrowPg = JSON.stringify({
    DataVersion: 14, PlayerID: midB, PlayerName: "BorrowerChan", SelectChara: 3,
    OdekakeChara: 3, BeforeChara: 0, OdekakeCharaCostume: lenderChara?.CostumeItemEquip,
    OdekakeCharaGameData: lenderChara, OdekakeCustomizeData: odStates.customize_item,
    PurchaseCharaList: [], GachaCharaUsableList: [],
    m_danLevel: 1, m_dojoSlotUnlocked: [true, false, false, false],
    CharaData: Array.from({ length: 21 }, (_, i) => ({ charaType: i, Level: 1, Exp: 0, IsDebut: false, CostumeItemEquip: 1000 + i })),
  });
  const bw = await post("client_state_write", { web_id: 1, area_id: 1, pcuid: pcB, mid: midB, kind: "player_game", data: b64json(borrowPg) });
  check("D6 the borrow survives the borrower's own save (server stores it as posted)", bw.code === 0, `code=${bw.code}`);
  const od6 = await post("client_state_read", { web_id: 1, area_id: 1, pcuid: pcB, mid: midB, one_kind: "player_game" });
  const servedPg = statesOf(od6.text).player_game;
  let pgDiff = "";
  try {
    const a = JSON.parse(borrowPg);
    for (const k of Object.keys(a)) {
      const x = JSON.stringify(a[k]), y = JSON.stringify(servedPg?.[k]);
      if (x !== y) pgDiff += `DIFF ${k}: posted=${String(x).slice(0, 90)} served=${String(y).slice(0, 90)} `;
    }
    for (const k of Object.keys(servedPg || {})) if (!(k in a)) pgDiff += `EXTRA ${k} `;
  } catch { pgDiff = "parse error"; }
  check("D7 the borrower's own saved borrow state round-trips unchanged (no read-time rewrite)",
    JSON.stringify(servedPg) === JSON.stringify(JSON.parse(borrowPg)), pgDiff || JSON.stringify(servedPg)?.slice(0, 180));
  const od7 = await post("odekake_done", { web_id: 1, area_id: 1, pcuid: pcB, odekake_id: ticket });
  check("D8 the borrow can be taken again from the same account", od7.code === 0 && /<success>1<\/success>/.test(od7.text));
  // the grant table is the only selector: the lender's mid, published or not, buys nothing.
  const odMid = await post("odekake_done", { web_id: 1, area_id: 1, pcuid: pcB, odekake_id: midL });
  check("D9 the ticket borrows, the lender's mid does not (grant table, not mid hash)",
    od.code === 0 && /<success>1<\/success>/.test(od.text) &&
    odMid.code === 0 && /<success>0<\/success>/.test(odMid.text) &&
    odGhost.code === 0 && /<success>0<\/success>/.test(odGhost.text),
    `ticket=${ticket} mid=${midL} midResult=${odMid.text.slice(0, 110)}`);

  // ---- E. events expose the borrow feature ----
  const info = await post("appli_info", { web_id: 1, area_id: 1 });
  const evRaw = Buffer.from((info.text.match(/<info_data kind="events">([^<]*)<\/info_data>/) || [])[1] || "", "base64").toString("utf-8");
  const ev = JSON.parse(evRaw);
  const odEvent = ev.list.find(x => x.name === "EnableOdekake");
  check("E1 EnableOdekake is advertised active", !!odEvent && odEvent.active === true, JSON.stringify(odEvent));

  // ---- F. shared scratch doc never written ----
  const { DatabaseSync } = require("node:sqlite");
  const db = new DatabaseSync(DBPATH, { readOnly: true });
  const profs = db.prepare("SELECT __refid, updatedAt, data FROM documents WHERE __s='plugins_profile'").all().map(r => {
    let d = {}; try { d = JSON.parse(r.data); } catch { }
    return {
      refid: r.__refid, name: d.name, updatedAt: r.updatedAt,
      dojo: Array.isArray(d.dojo) ? d.dojo.map(s => ({ available: !!s.available, chara: s.chara || "", stock: Number(s.stock || 0) })) : null,
    };
  });
  // The GUEST document is legacy (it predates the per-session dojo fix and the owner keeps
  // probe fixtures). What this run must not do is write through it: its updatedAt has to
  // predate the run.
  const guest = profs.find(p => p.refid === "GUEST");
  const guestTouched = guest && (() => {
    const t = Number.isFinite(Number(guest.updatedAt)) ? Number(guest.updatedAt) : Date.parse(String(guest.updatedAt));
    return Number.isFinite(t) && t >= STARTED_AT;
  })();
  check("F1 the dojo calls never wrote through the shared GUEST document", !guestTouched,
    guest ? `updatedAt=${guest.updatedAt} runStarted=${new Date(STARTED_AT).toISOString()}` : "no GUEST document");
  const lender = profs.find(p => p.refid === RL), borrower = profs.find(p => p.refid === RB);
  const lenderOk = lender?.dojo?.length === 4 && lender.dojo[0].available === true && lender.dojo[0].chara === "OID_CHARACTER_7"
    && lender.dojo.slice(1).every(s => !s.available);
  const borrowerOk = borrower?.dojo?.length === 4 && borrower.dojo.every(s => !s.available);
  check("F2 each real profile carries its own dojo array", lenderOk && borrowerOk,
    `lender=${JSON.stringify(lender?.dojo)} borrower=${JSON.stringify(borrower?.dojo)}`);
  db.close();

  console.log(fails ? `\n${fails} CHECK(S) FAILED` : "\nALL CHECKS PASSED");
  process.exitCode = fails ? 1 : 0;
})().catch(e => { console.error("probe error:", e); process.exitCode = 2; });
