// Driver for the scratch core-webui harness. Usage: node auth_driver.js <mode> <port>
const MODE = process.argv[2] || 'normal';
const PORT = Number(process.argv[3] || 18099);
const BASE = 'http://127.0.0.1:' + PORT;
const UA = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36';

let jar = '';
function sidOf(cookieHeader) {
  const m = /connect\.sid=([^;]*)/.exec(cookieHeader || '');
  return m ? m[1] : '';
}
async function req(method, p, { form, ip, cookie, headers } = {}) {
  const h = { 'User-Agent': UA, ...(headers || {}) };
  if (ip) h['X-Forwarded-For'] = ip;
  if (cookie) h['Cookie'] = cookie;
  if (form) h['Content-Type'] = 'application/x-www-form-urlencoded';
  const res = await fetch(BASE + p, { method, headers: h, body: form ? new URLSearchParams(form).toString() : undefined, redirect: 'manual' });
  const setc = res.headers.get('set-cookie') || '';
  let body = '';
  try { body = await res.text(); } catch { }
  return { status: res.status, location: res.headers.get('location'), setc, body };
}
const log = (...a) => console.log(a.join(' '));

async function normal() {
  log('=== T1 session fixation at signup / login ===');
  const g = await req('GET', '/signup', { ip: '10.0.0.1' });
  const pre = sidOf(g.setc);
  log('GET /signup status=' + g.status + ' pre-login sid=' + (pre ? pre.slice(0, 40) + '...' : '(none)'));
  const s = await req('POST', '/signup', { ip: '10.0.0.1', cookie: 'connect.sid=' + pre.split('=')[0] + '', form: { username: 'fixuser1', password: 'password123', confirmPassword: 'password123' } });
  // note: the signed cookie value must be passed verbatim, so rebuild it properly:
  log('  (re-send with the verbatim cookie)');
  const verbatim = 'connect.sid=' + pre;
  const s2 = await req('POST', '/signup', { ip: '10.0.0.1', cookie: verbatim, form: { username: 'fixuser2', password: 'password123', confirmPassword: 'password123' } });
  const post = sidOf(s2.setc);
  log('POST /signup status=' + s2.status + ' location=' + s2.location + ' post-login sid=' + (post ? post.slice(0, 40) + '...' : '(none)'));
  log('  pre sid == post sid ? ' + (pre && post ? (pre === post ? 'SAME SESSION ID (fixation)' : 'changed') : 'n/a'));
  jar = s2.setc;
  const me = await req('GET', '/api/me', { ip: '10.0.0.1', cookie: jar });
  log('GET /api/me status=' + me.status + ' body=' + me.body.slice(0, 160));

  log('=== T1b login keeps the same session id ===');
  const g2 = await req('GET', '/login', { ip: '10.0.0.2' });
  const pre2 = sidOf(g2.setc);
  const l = await req('POST', '/login', { ip: '10.0.0.2', cookie: 'connect.sid=' + pre2, form: { username: 'fixuser1', password: 'password123' } });
  const post2 = sidOf(l.setc);
  log('POST /login status=' + l.status + ' location=' + l.location + ' sid same as pre-login? ' + (pre2 === post2 ? 'SAME (fixation)' : 'changed') + (l.setc ? '' : ' [no Set-Cookie on POST /login]'));
  log('  pre=' + pre2.slice(0, 45) + '... post=' + (post2 ? post2.slice(0, 45) + '...' : '(none)'));
  const lcookie = l.setc || 'connect.sid=' + pre2;
  const me2 = await req('GET', '/api/me', { ip: '10.0.0.2', cookie: lcookie });
  log('GET /api/me after login status=' + me2.status + ' body=' + me2.body.slice(0, 160));

  log('=== T2 username race: 8 concurrent POST /signup, same username ===');
  const before = await req('GET', '/api/me', { ip: '10.0.9.9' });
  const races = await Promise.all(Array.from({ length: 8 }, (_, i) =>
    req('POST', '/signup', { ip: '10.0.' + (20 + i) + '.1', form: { username: 'raceduser', password: 'password' + (100 + i) + 'x', confirmPassword: 'password' + (100 + i) + 'x' } })
  ));
  log('statuses: ' + races.map(r => r.status).join(','));
  log('locations: ' + races.map(r => r.location || '-').join(','));
  log('errors rendered: ' + races.map(r => (r.body.match(/class="notification is-danger[^"]*"[^>]*>\s*([^<]{0,60})/) || [, ''])[1].trim()).join(' | '));

  log('=== T3 bcrypt 72-byte truncation ===');
  const A = 'A'.repeat(72) + '-tail-alpha-111111111111111111111111111111111111111111111111';
  const B = 'A'.repeat(72) + '-tail-BBETA-222222222222222222222222222222222222222222222222';
  log('password A length=' + A.length + ' B length=' + B.length + ' first72 equal? ' + (A.slice(0, 72) === B.slice(0, 72)));
  const sA = await req('POST', '/signup', { ip: '10.0.30.1', form: { username: 'truncuser', password: A, confirmPassword: A } });
  log('signup with A: status=' + sA.status + ' location=' + sA.location);
  const lB = await req('POST', '/login', { ip: '10.0.30.2', cookie: 'connect.sid=' + sidOf((await req('GET', '/login', { ip: '10.0.30.2' })).setc), form: { username: 'truncuser', password: B } });
  const lBcookie = lB.setc || undefined;
  const me3 = lBcookie ? await req('GET', '/api/me', { ip: '10.0.30.2', cookie: lBcookie }) : { status: 0, body: 'no cookie' };
  log('login with B (different tail): status=' + lB.location + ' then /api/me status=' + me3.status + ' body=' + String(me3.body).slice(0, 140));

  log('=== T4 username validation: trailing newline ===');
  const nl = await req('POST', '/signup', { ip: '10.0.31.1', form: { username: 'nluser\n', password: 'password123', confirmPassword: 'password123' } });
  log('signup username="nluser\\n": status=' + nl.status + ' location=' + (nl.location || '-') + ' error=' + JSON.stringify((nl.body.match(/class="notification is-danger[^"]*"[^>]*>\s*([^<]{0,80})/) || [, ''])[1].trim()));

  log('=== T5 rate limit keyed by X-Forwarded-For ===');
  const burst = await Promise.all(Array.from({ length: 12 }, () => req('POST', '/login', { ip: '10.0.40.1', form: { username: 'nobody', password: 'nope12345' } })));
  log('same-IP 12 POST /login statuses: ' + burst.map(r => r.status).join(','));
  const other = await req('POST', '/login', { ip: '10.0.41.1', form: { username: 'nobody', password: 'nope12345' } });
  log('different-IP status=' + other.status);
  const spoof1 = await req('POST', '/login', { ip: '198.51.100.7', form: { username: 'nobody', password: 'nope12345' } });
  const spoof2 = await req('POST', '/login', { ip: '198.51.100.8', form: { username: 'nobody', password: 'nope12345' } });
  log('varying XFF bypasses limit? statuses=' + spoof1.status + ',' + spoof2.status);
}

async function fixed() {
  log('=== T6 collision acceptance (mint RNG pinned to 0200000000000000) ===');
  const rs = await Promise.all(Array.from({ length: 4 }, (_, i) =>
    req('POST', '/signup', { ip: '10.1.' + i + '.1', form: { username: 'coll' + i, password: 'password123', confirmPassword: 'password123' } })
  ));
  log('statuses=' + rs.map(r => r.status).join(',') + ' locations=' + rs.map(r => r.location || '-').join(','));
  log('errors=' + rs.map(r => (r.body.match(/class="notification is-danger[^"]*"[^>]*>\s*([^<]{0,80})/) || [, ''])[1].trim()).join(' | '));
}

async function reserved() {
  log('=== T7 reserved-shape exclusion (mint RNG pinned to ' + (process.env.FIXED_RANDOM || '') + ') ===');
  const r = await req('POST', '/signup', { ip: '10.2.0.1', form: { username: 'reserved1', password: 'password123', confirmPassword: 'password123' } });
  log('status=' + r.status + ' location=' + (r.location || '-'));
  log('rendered error=' + JSON.stringify((r.body.match(/class="notification is-danger[^"]*"[^>]*>\s*([^<]{0,90})/) || [, ''])[1].trim()));
}

(async () => {
  if (MODE === 'fixed') await fixed();
  else if (MODE === 'reserved') await reserved();
  else await normal();
})().catch(e => log('DRIVER ERROR: ' + (e && e.stack ? e.stack : e)));
