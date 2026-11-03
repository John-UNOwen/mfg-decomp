// Syntax/transpile check for the mfg@asphyxia plugin sources (the core loads
// plugins through ts-node transpileOnly, so this is the same front end).
// usage: node .audit/ts_check_plugin.js [file...]
const fs = require("fs");
const path = require("path");
const ts = require(path.join(__dirname, "..", "tmp", "RyuNET-core", "node_modules", "typescript"));

const root = path.join(__dirname, "..", "tmp", "asphyxia_plugins", "mfg@asphyxia");
const files = process.argv.slice(2).length
  ? process.argv.slice(2)
  : ["index.ts", "mahjong.ts", "taikyoku.ts", "handlers/aog.ts", "handlers/utils.ts", "handlers/eamuse.ts"]
      .map((f) => path.join(root, f));

let bad = 0;
for (const f of files) {
  const src = fs.readFileSync(f, "utf8");
  const out = ts.transpileModule(src, {
    fileName: f,
    compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022 },
    reportDiagnostics: true,
  });
  const diags = (out.diagnostics || []).filter((d) => d.category === 1);
  if (diags.length) {
    bad += diags.length;
    for (const d of diags) {
      const pos = d.file && d.start != null ? d.file.getLineAndCharacterOfPosition(d.start) : null;
      console.log(`ERROR ${path.basename(f)}:${pos ? pos.line + 1 : "?"} ${ts.flattenDiagnosticMessageText(d.messageText, " ")}`);
    }
  } else {
    console.log(`ok ${path.basename(f)}`);
  }
}
process.exit(bad ? 1 : 0);
