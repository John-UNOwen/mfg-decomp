import fs from 'node:fs';
const files = ['eamuse/ExternalPluginLoader.js', 'webui/index.js', 'webui/routes/user.js'];
for (const rel of files) {
  const read = (p) => fs.readFileSync(p, 'utf8').replace(/\r\n/g, '\n').split('\n').filter((l) => !/^\/\/# sourceMappingURL=/.test(l));
  const a = read('C:/mfg/tmp/RyuNET-core/dist/' + rel);
  const b = read('C:/mfg/scratch/audit_tsc_out/' + rel);
  const crlfLive = fs.readFileSync('C:/mfg/tmp/RyuNET-core/dist/' + rel, 'utf8').includes('\r\n');
  const crlfBuilt = fs.readFileSync('C:/mfg/scratch/audit_tsc_out/' + rel, 'utf8').includes('\r\n');
  console.log('==== ' + rel + ' liveLines=' + a.length + ' builtLines=' + b.length + ' liveCRLF=' + crlfLive + ' ====');
  // simple LCS-based diff (small files)
  const n = a.length, m = b.length;
  const dp = Array.from({ length: n + 1 }, () => new Int32Array(m + 1));
  for (let i = n - 1; i >= 0; i--) for (let j = m - 1; j >= 0; j--)
    dp[i][j] = a[i] === b[j] ? dp[i + 1][j + 1] + 1 : Math.max(dp[i + 1][j], dp[i][j + 1]);
  let i = 0, j = 0, shown = 0;
  while (i < n && j < m && shown < 40) {
    if (a[i] === b[j]) { i++; j++; continue; }
    if (dp[i + 1][j] >= dp[i][j + 1]) { console.log('ONLY LIVE  @' + (i + 1) + ': ' + a[i]); i++; }
    else { console.log('ONLY BUILT @' + (j + 1) + ': ' + b[j]); j++; }
    shown++;
  }
  while (i < n && shown < 40) { console.log('ONLY LIVE(tail) @' + (i + 1) + ': ' + a[i]); i++; shown++; }
  while (j < m && shown < 40) { console.log('ONLY BUILT(tail) @' + (j + 1) + ': ' + b[j]); j++; shown++; }
}
