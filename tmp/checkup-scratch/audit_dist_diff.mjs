import fs from 'node:fs';
const files = ['eamuse/ExternalPluginLoader.js', 'webui/index.js', 'webui/routes/user.js'];
for (const rel of files) {
  const a = fs.readFileSync('C:/mfg/tmp/RyuNET-core/dist/' + rel, 'utf8').split('\n').filter((l) => !/^\/\/# sourceMappingURL=/.test(l));
  const b = fs.readFileSync('C:/mfg/scratch/audit_tsc_out/' + rel, 'utf8').split('\n').filter((l) => !/^\/\/# sourceMappingURL=/.test(l));
  console.log('==== ' + rel + ' live=' + a.length + ' built=' + b.length + ' ====');
  const max = Math.max(a.length, b.length);
  let shown = 0;
  for (let i = 0; i < max && shown < 60; i++) {
    if ((a[i] || '') !== (b[i] || '')) {
      console.log('L' + (i + 1) + ' LIVE : ' + (a[i] || '<none>'));
      console.log('L' + (i + 1) + ' BUILT: ' + (b[i] || '<none>'));
      shown++;
    }
  }
}
