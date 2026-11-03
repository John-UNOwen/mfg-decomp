// Read-only peek: schema + the probe profiles' notice state in the live plugin DB.
const { DatabaseSync } = require("node:sqlite");
const DB_PATH = process.argv[2] || "C:\\mfg\\tmp\\RyuNET-core\\dist\\savedata\\mfg@asphyxia.db";
const db = new DatabaseSync(DB_PATH, { readOnly: true });
const tables = db.prepare("SELECT name FROM sqlite_master WHERE type='table'").all();
console.log("tables:", tables.map(t => t.name).join(","));
for (const t of tables) {
  const cols = db.prepare(`PRAGMA table_info(${t.name})`).all().map(c => c.name);
  console.log(`  ${t.name}: ${cols.join(",")}`);
}
const table = tables.some(t => t.name === "documents") ? "documents" : (tables[0] && tables[0].name);
const rows = db.prepare(`SELECT * FROM ${table}`).all();
console.log("rows in", table, "=", rows.length);
for (const r of rows) {
  const payload = r.payload || r.data || r.value || "";
  if (!String(payload).includes("notice")) continue;
  let doc;
  try { doc = JSON.parse(payload); } catch { continue; }
  const idTag = JSON.stringify(doc.refid || doc.player_id || r.__refid || r._id);
  if (!/P8|GUEST/.test(String(idTag))) continue;
  console.log("=== profile", idTag, "name", doc.name, "guest", doc.guest);
  console.log("   notice_done_ids:", JSON.stringify(doc.notice_done_ids));
  console.log("   important_notice_done_ids:", JSON.stringify(doc.important_notice_done_ids));
  const st = doc.states || {};
  console.log("   state kinds:", Object.keys(st).sort().join(","));
  for (const k of ["notice_game", "receive_only"]) {
    console.log("   " + k + " ->", String(st[k] === undefined ? "<absent>" : st[k]).slice(0, 240));
  }
}
db.close();
