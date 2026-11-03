// Unit-level check of the P8 config overrides (VFG_MISSION_ACTIVE / VFG_EVENTS_ACTIVE /
// VFG_EVENTS_OFF / VFG_TABOO_EXTRA). configList() reads the core config first and the
// environment second, so setting the environment before requiring handlers/utils.ts
// exercises the same code path a config.ini value would - no server, no restart.
// Same expectations as tmp/p8repro/verify_p8_config.py, which needs a configured server.
const path = require("path");
const ROOT = "C:\\mfg\\tmp\\RyuNET-core";
process.env.VFG_MISSION_ACTIVE = "Bomb,Competition9";
process.env.VFG_EVENTS_ACTIVE = "Competition9,BombTakuEvent=Rank=2,Bomb";
process.env.VFG_EVENTS_OFF = "CostumeAddition1";
process.env.VFG_TABOO_EXTRA = "zzzq";

require(path.join(ROOT, "node_modules", "ts-node")).register({
  transpileOnly: true,
  compilerOptions: { target: "es2020", module: "commonjs", moduleResolution: "node", skipLibCheck: true },
});
const U = require(path.join(ROOT, "dist", "plugins", "mfg@asphyxia", "handlers", "utils.ts"));

const fails = [];
function check(name, ok, detail) {
  console.log((ok ? "PASS  " : "FAIL  ") + name + (detail ? "   " + detail : ""));
  if (!ok) fails.push(name);
}

const mi = JSON.parse(U.missionsJson()).list;
const mon = new Set(mi.filter(r => r.active).map(r => r.category));
check("VFG_MISSION_ACTIVE switches dated categories on", ["Bomb", "Competition9"].every(n => mon.has(n)), "active=" + [...mon].join(","));
check("VFG_MISSION_ACTIVE adds to the defaults", ["JudgmentDaily", "Player", "StartDash"].every(n => mon.has(n)));
check("categories not named stay off", !["Aotenjo2025", "Comeback2", "Atomic"].some(n => mon.has(n)));
check("the schedule still covers all 103 categories", mi.length === 103, "rows=" + mi.length);

const ev = JSON.parse(U.eventsJson()).list;
const by = new Map(ev.map(r => [r.name, r]));
check("VFG_EVENTS_ACTIVE switches an event kind on", by.get("Competition9") && by.get("Competition9").active === true);
check("VFG_EVENTS_ACTIVE carries the =param through", by.get("BombTakuEvent") && by.get("BombTakuEvent").param === "Rank=2" && by.get("BombTakuEvent").active === true, "row=" + JSON.stringify(by.get("BombTakuEvent")));
check("a name that is not a GameEventType member is not emitted", !by.has("Bomb"), "VFG_EVENTS_ACTIVE also carried Bomb, a Mission.Category name");
check("VFG_EVENTS_OFF wins over the built-in set", by.get("CostumeAddition1") && by.get("CostumeAddition1").active === false && by.get("CostumeAddition2") && by.get("CostumeAddition2").active === true);
check("the full kind list is unaffected by the overrides", ev.length === 304, "rows=" + ev.length);

for (const [word, want] of [["my zzzq name", true], ["zzzq", true], ["ordinary name", false]]) {
  const hit = U.isTabooText(word);
  check("VFG_TABOO_EXTRA blocks '" + word + "'", !!hit === want, "matched=" + JSON.stringify(hit));
}
const tabooFile = JSON.parse(require("fs").readFileSync(path.join(ROOT, "dist", "plugins", "mfg@asphyxia", "data", "taboowords.json"), "utf8"));
check("TABOO_WORDS = file list + extra", U.TABOO_WORDS.length === tabooFile.words.length + 1, `words=${U.TABOO_WORDS.length} file=${tabooFile.words.length} extra=1 (VFG_TABOO_EXTRA=zzzq)`);

console.log("\n" + (fails.length ? fails.length + " FAILED: " + fails.join(", ") : "ALL CONFIG CHECKS PASSED"));
process.exit(fails.length ? 1 : 0);
