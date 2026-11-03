// Scratch probe: dump profile rows relevant to dojo / friend_game / odekake.
// usage: node probe_p7.js [path-to-mfg@asphyxia.db] [space]
// Default is the canonical tree's own DB (tmp/RyuNET-core/dist/savedata). A relative
// "./savedata" default used to mean "whichever lane directory you happened to cd into",
// which is how a probe read the state of a stale build. Frozen snapshots have no savedata.
const path = process.argv[2] || "C:/mfg/tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db";
const space = process.argv[3] || "profile";
const { DatabaseSync } = require("node:sqlite");
console.log("db:", require("path").resolve(path));
const db = new DatabaseSync(path, { readOnly: true });

const tables = db.prepare("SELECT name FROM sqlite_master WHERE type='table'").all();
console.log("tables:", tables.map(t => t.name).join(", "));
const spaces = db.prepare("SELECT DISTINCT __s FROM documents").all().map(r => r.__s);
console.log("spaces:", spaces.join(", "));

const rows = db.prepare("SELECT __refid, data FROM documents WHERE __s=? ORDER BY __refid").all(space);
console.log(`\n${space} rows: ${rows.length}`);
for (const r of rows) {
  let data = {};
  try { data = JSON.parse(r.data || "{}"); } catch { }
  const states = data.states || {};
  const dojo = Array.isArray(data.dojo)
    ? data.dojo.map(s => `${s.available ? "A" : "-"}:${s.chara || ""}:${s.stock}`).join("|")
    : "none";
  console.log(
    [
      "refid=" + r.__refid,
      "name=" + data.name,
      "mid=" + data.player_id,
      "session=" + String(data.session_id || "").slice(0, 8),
      "dojo=" + dojo,
      "states=" + Object.keys(states).sort().join(","),
    ].join("  ")
  );
  if (states.friend_game) console.log("   friend_game: " + String(states.friend_game).slice(0, 400));
}
