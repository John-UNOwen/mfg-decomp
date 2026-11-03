// Matcher comparison: the two builds the p10 repro transpiled (run/pre = before the
// narrowing, run/post = after), over one probe corpus. Prints every probe whose
// verdict changed, split into "narrowed" (refused before, accepted now) and
// "added" (accepted before, refused now).
'use strict';
globalThis.U = { GetConfig: () => undefined };
globalThis.K = {};
globalThis.IO = { ReadFile: () => null, Resolve: () => '' };

const pre = require('../tmp/p10taboo/run/pre/dist/plugins/mfg@asphyxia/handlers/utils.js');
const post = require('../tmp/p10taboo/run/post/dist/plugins/mfg@asphyxia/handlers/utils.js');

const names = [
  // names an account may honestly have
  'カスミ', 'カスミサキ', 'Kasumi Aoi', 'マスカット', 'クスコ', 'ブシドウ', 'バスケ',
  'Grape', 'Grape Aoi', 'Grapefruit', 'Drapes', 'Scamp', 'Scamper', 'Escarp', 'Scarlet',
];
const profane = [
  'rape', 'raped', 'rapes', 'raping', 'rapist', 'rape lord', 'rape-lord', 'rape1234', '1234rape',
  'fuck', 'fucks', 'fucked', 'fucking', 'motherfuck', 'motherfucker', 'fuckyou', 'youfuck',
  'cunt', 'cunts', 'cuntface', 'slut', 'sluts', 'slutty', 'whore', 'whores', 'bitch', 'bitchy',
  'retard', 'retards', 'asshole', 'assholes', 'molest', 'molested', 'pedophile', 'pedophilia',
  'lolicon', 'childsex', 'terrorist', 'terrorists', 'phishing', 'scam', 'scams', 'scammer', 'scamming',
  '死ね', '死ねば', '殺す', '殺すな', '消えろ', '消えろよ', 'カス', 'カスミ', 'ブス', 'クソ', 'キモイ',
  'レイプ', '売春', '売春婦', '援助交際', '援助交際希望', '性処理', '奴隷', '奴隷状態', '傻逼', '去死',
];

const probes = [...new Set([...names, ...profane])];
const narrowed = [], added = [];
for (const p of probes) {
  const a = pre.isTabooText(p), b = post.isTabooText(p);
  if (a && !b) narrowed.push([p, a]);
  else if (!a && b) added.push([p, b]);
}
const fmt = (rows) => rows.map(([p, w]) => `    ${JSON.stringify(p).padEnd(18)} was '${w}'`).join('\n');
console.log(`probes=${probes.length}  verdict changed: narrowed ${narrowed.length}, added ${added.length}\n`);
console.log('REFUSED BEFORE, ACCEPTED NOW (the narrowing: entry embedded in a longer word):');
console.log(fmt(narrowed));
console.log('\nACCEPTED BEFORE, REFUSED NOW (the entries the data file gained):');
console.log(fmt(added));
