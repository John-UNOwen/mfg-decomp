import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const distRoot = 'C:/mfg/tmp/RyuNET-core/dist';
const outRoot = 'C:/mfg/scratch/audit_tsc_out';
const skip = (p) => p.startsWith('plugins') || p.startsWith('savedata') || p.startsWith('uploads');

function walk(root, base = '') {
  const res = [];
  for (const e of fs.readdirSync(path.join(root, base), { withFileTypes: true })) {
    const rel = base ? base + '/' + e.name : e.name;
    if (e.isDirectory()) res.push(...walk(root, rel));
    else if (e.name.endsWith('.js')) res.push(rel);
  }
  return res;
}
function norm(f) {
  let s = fs.readFileSync(f, 'utf8');
  s = s.split('\n').filter((l) => !/^\/\/# sourceMappingURL=/.test(l)).join('\n').replace(/\s+$/, '');
  return s;
}
const hash = (f) => crypto.createHash('sha256').update(norm(f)).digest('hex');

const built = new Set(walk(outRoot));
const live = walk(distRoot).filter((r) => !skip(r));
let same = 0;
const diffs = [];
const onlyLive = [];
for (const rel of live) {
  if (!built.has(rel)) { onlyLive.push(rel); continue; }
  if (hash(path.join(distRoot, rel)) === hash(path.join(outRoot, rel))) same++;
  else diffs.push(rel);
}
const onlyBuilt = [...built].filter((r) => !live.includes(r));
console.log(JSON.stringify({ liveCoreJs: live.length, same, diffs, onlyLive, onlyBuilt }, null, 1));
