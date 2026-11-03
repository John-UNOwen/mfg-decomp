// Unit-level check of the shipped plugin content code, no server, no DB, no ports.
// Loads dist/plugins/mfg@asphyxia/handlers/utils.ts the way the core does (ts-node)
// and prints what the content helpers actually return from data/*.json.
const path = require("path");
const ROOT = "C:\\mfg\\tmp\\RyuNET-core";
require(path.join(ROOT, "node_modules", "ts-node")).register({
  transpileOnly: true,
  compilerOptions: { target: "es2020", module: "commonjs", moduleResolution: "node", skipLibCheck: true },
});

const U = require(path.join(ROOT, "dist", "plugins", "mfg@asphyxia", "handlers", "utils.ts"));

const ev = JSON.parse(U.eventsJson());
const mi = JSON.parse(U.missionsJson());
const ro = JSON.parse(U.receiveOnlyJson(null));
const ng = JSON.parse(U.noticeGameJson(null));

console.log("TABOO_WORDS        :", U.TABOO_WORDS.length, U.TABOO_WORDS.slice(0, 5).join("/"));
console.log("isTabooText('FUCK you'):", U.isTabooText("FUCK you"));
console.log("isTabooText('MeldBonus'):", U.isTabooText("MeldBonus"));
console.log("event rows         :", ev.list.length, "active:", ev.list.filter(r => r.active).length);
console.log("mission rows       :", mi.list.length, "active:", mi.list.filter(r => r.active).map(r => r.category).join(","));
console.log("serverNoticeDocs   :", U.serverNoticeDocs().length, U.serverNoticeDocs().map(n => n.NoticeID).join(","));
console.log("importantNoticeIds :", U.importantNoticeIds().join(","));
console.log("noticeGameJson     :", ng.NoticeInfos.length, "ids:", ng.NoticeInfos.map(n => n.NoticeID).join(","));
console.log("receiveOnlyJson    :", ro.ImportantNotices.length, "ids:", ro.ImportantNotices.map(n => n.NoticeID).join(","));
console.log("SERVER_OWNED_KINDS :", U.SERVER_OWNED_STATE_KINDS.join(","));
for (const k of U.SERVER_OWNED_STATE_KINDS) {
  const doc = U.serverOwnedStateJson(k, null);
  console.log("  serverOwnedStateJson(" + k + ") ->", doc === null ? "null" : doc.slice(0, 90));
}
