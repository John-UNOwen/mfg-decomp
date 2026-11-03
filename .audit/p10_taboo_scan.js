// Read-only scan of the live plugin savedata: which stored player names would be
// refused by the client_state_write profanity gate (substring rule, as shipped),
// and which of those survive a word-boundary rule.
const { DatabaseSync } = require("node:sqlite");
const fs = require("fs");

const DB_PATH = process.argv[2] || "C:\\mfg\\tmp\\RyuNET-core\\dist\\savedata\\mfg@asphyxia.db";
const WORDS = JSON.parse(fs.readFileSync("C:\\mfg\\tmp\\RyuNET-core\\dist\\plugins\\mfg@asphyxia\\data\\taboowords.json", "utf8"))
  .words.map((w) => String(w).trim().toLowerCase()).filter(Boolean);

// shipped rule: case-insensitive substring (handlers/utils.ts isTabooText)
const substringHit = (s) => {
  const v = String(s || "").trim().toLowerCase();
  for (const w of WORDS) if (v.includes(w)) return w;
  return null;
};
// proposed rule: short entries (<= 4) match only on word boundaries
const BOUND_MAX = 4;
const BOUNDARY = String.raw`[\p{L}\p{N}]`;
const boundaryHit = (s) => {
  const v = String(s || "").trim().toLowerCase();
  for (const w of WORDS) {
    if (w.length > BOUND_MAX) { if (v.includes(w)) return w; continue; }
    const re = new RegExp(`(?<!${BOUNDARY})${w.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}(?!${BOUNDARY})`, "iu");
    if (re.test(v)) return w;
  }
  return null;
};

const db = new DatabaseSync(DB_PATH, { readOnly: true });
const tables = db.prepare("SELECT name FROM sqlite_master WHERE type='table'").all().map((t) => t.name);
const table = tables.includes("documents") ? "documents" : tables[0];
const rows = db.prepare(`SELECT * FROM ${table}`).all();
db.close();

let profiles = 0, flagged = 0, stillFlagged = 0;
for (const r of rows) {
  const payload = r.payload || r.data || r.value || "";
  let doc;
  try { doc = JSON.parse(payload); } catch { continue; }
  if (!doc || doc.collection !== "profile") continue;
  profiles++;
  const st = doc.states || {};
  let pgName = null;
  try { pgName = JSON.parse(String(st.player_game || "{}")).PlayerName ?? null; } catch { }
  const names = [doc.name, pgName].filter((x) => typeof x === "string" && x.trim());
  const uniq = [...new Set(names.map((x) => x.trim()))];
  for (const n of uniq) {
    const h = substringHit(n);
    if (!h) continue;
    flagged++;
    const survives = boundaryHit(n);
    if (survives) stillFlagged++;
    console.log(`  profile ${String(doc.refid).padEnd(18)} mid=${String(doc.player_id).padEnd(10)} name=${JSON.stringify(n)} stored_PlayerName=${JSON.stringify(pgName)} word=${JSON.stringify(h)} boundary_rule=${survives ? "STILL BLOCKED" : "released"}`);
  }
}
console.log(`profiles scanned=${profiles} stored names with a taboo substring hit=${flagged} of them still blocked by the boundary rule=${stillFlagged}`);
console.log(`word list: ${WORDS.length} entries, short (<=${BOUND_MAX}): ${WORDS.filter((w) => w.length <= BOUND_MAX).map((w) => `${w}(${w.length})`).join(" ")}`);
