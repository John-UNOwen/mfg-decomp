// Lane probes: (1) collision refusal for a normalized name, (2) concurrent signup
// serialization, (3) legacy mixed-case collision takeover.
const http = require('http');
const HOST = '127.0.0.1', PORT = 8191;

function req(method, path, { form, cookie, xff } = {}) {
  return new Promise((resolve, reject) => {
    const body = form ? new URLSearchParams(form).toString() : null;
    const headers = {
      'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36',
      'Accept': 'text/html,application/xhtml+xml',
    };
    if (body) { headers['Content-Type'] = 'application/x-www-form-urlencoded'; headers['Content-Length'] = Buffer.byteLength(body); }
    if (cookie) headers['Cookie'] = cookie;
    if (xff) headers['X-Forwarded-For'] = xff;
    const r = http.request({ host: HOST, port: PORT, path, method, headers }, (res) => {
      let d = '';
      res.on('data', (c) => (d += c));
      res.on('end', () => resolve({ status: res.statusCode, location: res.headers.location || null, setCookie: res.headers['set-cookie'] || null, body: d }));
    });
    r.on('error', reject);
    if (body) r.write(body);
    r.end();
  });
}
const brief = (r) => ({ status: r.status, location: r.location, msg: /class="notification is-danger[^"]*"[^>]*>\s*([^<]{0,80})/.exec(r.body)?.[1] || null });

(async () => {
  const out = {};
  const s1 = await req('POST', '/signup', { form: { username: 'ZetaUser', password: 'Passw0rd123', confirmPassword: 'Passw0rd123' }, xff: '10.30.0.1' });
  out.signup_ZetaUser = brief(s1);
  const s2 = await req('POST', '/signup', { form: { username: 'ZETAUSER', password: 'Passw0rd999', confirmPassword: 'Passw0rd999' }, xff: '10.30.0.2' });
  out.signup_ZETAUSER_duplicate = brief(s2);
  const s3 = await req('POST', '/signup', { form: { username: ' zetauser ', password: 'Passw0rd999', confirmPassword: 'Passw0rd999' }, xff: '10.30.0.3' });
  out.signup_padded_space = brief(s3);

  // concurrent signups for the same username, one limiter bucket each
  const N = 12;
  const race = await Promise.all(Array.from({ length: N }, (_, i) =>
    req('POST', '/signup', { form: { username: 'raceuser', password: 'RacePass123', confirmPassword: 'RacePass123' }, xff: '10.31.' + i + '.1' })));
  out.race_statuses = race.map((r) => r.status);
  out.race_redirects = race.filter((r) => r.status === 302 && r.location === '/about').length;
  out.race_rejected = race.filter((r) => r.body.includes('already exists')).length;
  out.race_other = race.filter((r) => r.status !== 302 && !r.body.includes('already exists')).map((r) => r.status);
  console.log(JSON.stringify(out, null, 1));
})();
