// Rate-limit key semantics: which entry of X-Forwarded-For does express-rate-limit bucket on?
const PORT = Number(process.argv[2] || 18099);
const BASE = 'http://127.0.0.1:' + PORT;
const UA = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36';
async function post(ip) {
  const res = await fetch(BASE + '/login', {
    method: 'POST',
    headers: { 'User-Agent': UA, 'Content-Type': 'application/x-www-form-urlencoded', 'X-Forwarded-For': ip },
    body: new URLSearchParams({ username: 'nosuchuser', password: 'wrongpass1' }).toString(),
    redirect: 'manual',
  });
  return res.status;
}
async function batch(xff, n = 12) {
  const out = [];
  for (let i = 0; i < n; i++) out.push(await post(xff));
  return out;
}
(async () => {
  console.log('A  XFF="198.51.100.20, 203.0.113.5"  -> ' + (await batch('198.51.100.20, 203.0.113.5')).join(','));
  console.log('B  XFF="198.51.100.21, 203.0.113.5"  -> ' + (await batch('198.51.100.21, 203.0.113.5')).join(','));
  console.log('C  XFF="198.51.100.20"               -> ' + (await batch('198.51.100.20')).join(','));
  console.log('D  XFF="198.51.100.22, 203.0.113.6"  -> ' + (await batch('198.51.100.22, 203.0.113.6')).join(','));
})();
