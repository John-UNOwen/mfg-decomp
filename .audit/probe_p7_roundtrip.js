// Scratch: compare a posted player_game with what the read serves back.
// Lane default is the staged verify-dist port (pwsh scripts/ryunet_stage.ps1 -Action lane).
// 8095 was the verify-p7 copy of dist, frozen at 01:54 and retired to .frozen-builds.
const BASE = (process.argv[2] || "http://127.0.0.1:8093") + "/aog/";
const b64json = (s) => Buffer.from(s, "utf-8").toString("base64");
async function post(name, body) {
  const res = await fetch(BASE + name, { method: "POST", headers: { "content-type": "application/x-www-form-urlencoded", "user-agent": "curl/8" }, body: new URLSearchParams(body).toString() });
  return await res.text();
}
(async () => {
  const login = await post("login", { web_id: 1, area_id: 1, user_id: "P7ROUNDTRIP" });
  const pc = (login.match(/<session_id>([0-9a-f]+)<\/session_id>/) || [])[1];
  const menu = await post("get_menudata", { web_id: 1, area_id: 1, pcuid: pc });
  const mid = Number(menu.match(/<mid>(\d+)<\/mid>/)?.[1]);
  const pg = JSON.stringify({
    DataVersion: 14, PlayerID: mid, PlayerName: "RoundTrip", SelectChara: 3,
    OdekakeChara: 3, BeforeChara: 0, OdekakeCharaCostume: 1003,
    OdekakeCharaGameData: { charaType: 3, Level: 1, Exp: 0, IsDebut: true, CostumeItemEquip: 1003 },
    OdekakeCustomizeData: { DataVersion: 1, CustomizeItems: [] },
    m_danLevel: 1, m_dojoSlotUnlocked: [true, false, false, false],
    CharaData: Array.from({ length: 21 }, (_, i) => ({ charaType: i, Level: 1, Exp: 0, IsDebut: false, CostumeItemEquip: 1000 + i })),
  });
  const w = await post("client_state_write", { web_id: 1, area_id: 1, pcuid: pc, mid, kind: "player_game", data: b64json(pg) });
  console.log("write response:", w.replace(/\s+/g, " ").slice(0, 200));
  const r = await post("client_state_read", { web_id: 1, area_id: 1, pcuid: pc, mid, one_kind: "player_game" });
  const chunk = (r.match(/<state kind="player_game"><data>([^<]*)<\/data><\/state>/) || [])[1] || "";
  const served = Buffer.from(chunk, "base64").toString("utf-8");
  const a = JSON.parse(pg), b = JSON.parse(served);
  console.log("posted === served(raw):", pg === served);
  console.log("posted === served(parsed):", JSON.stringify(a) === JSON.stringify(b));
  const keys = new Set([...Object.keys(a), ...Object.keys(b)]);
  for (const k of keys) {
    const x = JSON.stringify(a[k]), y = JSON.stringify(b[k]);
    if (x !== y) console.log(`DIFF ${k}: posted=${String(x).slice(0, 160)} served=${String(y).slice(0, 160)}`);
  }
})().catch(e => { console.error(e); process.exitCode = 2; });
