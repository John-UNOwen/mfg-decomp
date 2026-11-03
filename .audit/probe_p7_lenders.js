// Scratch probe: lender candidacy view over plugin profiles.
// usage: node probe_p7_lenders.js [db] [space]
// Default is the canonical tree's own DB - a relative default used to read whichever lane
// directory the probe happened to be run from.
const path = process.argv[2] || "C:/mfg/tmp/RyuNET-core/dist/savedata/mfg@asphyxia.db";
const space = process.argv[3] || "plugins_profile";
const { DatabaseSync } = require("node:sqlite");
console.log("db:", require("path").resolve(path));
const db = new DatabaseSync(path, { readOnly: true });
const rows = db.prepare("SELECT __refid, data FROM documents WHERE __s=? ORDER BY __refid").all(space);
for (const r of rows) {
  let data = {};
  try { data = JSON.parse(r.data || "{}"); } catch { }
  const st = data.states || {};
  let pg = null;
  try { pg = JSON.parse(st.player_game || "null"); } catch { }
  const cd = pg && Array.isArray(pg.CharaData) ? pg.CharaData.length : -1;
  console.log(
    [
      "refid=" + r.__refid,
      "name=" + JSON.stringify(data.name),
      "mid=" + data.player_id,
      "SelectChara=" + (pg ? pg.SelectChara : "n/a"),
      "CharaData=" + cd,
      "customize_item=" + (st.customize_item ? "yes" : "no"),
      "player_game=" + (st.player_game ? "yes" : "no"),
    ].join("  ")
  );
}
