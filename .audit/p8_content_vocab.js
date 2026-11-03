// Loads a plugin utils.ts snapshot the way the core loads it and prints the content
// vocabulary it actually ships: event kinds, mission categories, taboo words, notices.
// argv[1] = path to the utils.ts to inspect (defaults to the live dist build).
const path = require("path");
const ROOT = "C:\\mfg\\tmp\\RyuNET-core";
require(path.join(ROOT, "node_modules", "ts-node")).register({
  transpileOnly: true,
  compilerOptions: { target: "es2020", module: "commonjs", moduleResolution: "node", skipLibCheck: true },
});

const target = process.argv[2] || path.join(ROOT, "dist", "plugins", "mfg@asphyxia", "handlers", "utils.ts");
console.log("snapshot:", target);
const U = require(target);

const names = new Set((U.ACTIVE_EVENTS || []).map(e => e[0]));
console.log("ACTIVE_EVENTS      :", (U.ACTIVE_EVENTS || []).length, "entries,", names.size, "unique names");
console.log("MISSION_CATEGORIES :", (U.MISSION_CATEGORIES || []).length, (U.MISSION_CATEGORIES || []).map(c => c[1]).join(","));
console.log("TABOO_WORDS        :", (U.TABOO_WORDS || []).length);
for (const fn of ["eventKindNames", "serverNoticeDocs", "importantNoticeIds", "noticeGameJson", "receiveOnlyJson", "serverOwnedStateJson"]) {
  console.log(String(fn) + " present:", typeof U[fn] === "function");
}
if (typeof U.eventsJson === "function") {
  const ev = JSON.parse(U.eventsJson());
  console.log("eventsJson rows    :", ev.list.length, "active:", ev.list.filter(r => r.active).length);
}
if (typeof U.missionsJson === "function") {
  const mi = JSON.parse(U.missionsJson());
  console.log("missionsJson rows  :", mi.list.length, "active:", mi.list.filter(r => r.active).length);
}
if (typeof U.serverNoticeDocs === "function") {
  console.log("serverNoticeDocs   :", U.serverNoticeDocs().length, "important:", (U.importantNoticeIds ? U.importantNoticeIds() : []).join(","));
}
