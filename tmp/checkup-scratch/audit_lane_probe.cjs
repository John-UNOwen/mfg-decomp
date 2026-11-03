// Identity probes against the scratch audit lane (a copy of the live dist tree).
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
      res.on('end', () => resolve({
        status: res.statusCode,
        location: res.headers.location || null,
        setCookie: res.headers['set-cookie'] || null,
        body: d,
      }));
    });
    r.on('error', reject);
    if (body) r.write(body);
    r.end();
  });
}
const sidOf = (res) => (res.setCookie || []).map((c) => c.split(';')[0]).find((c) => c.startsWith('connect.sid=')) || null;
const summary = (r) => ({ status: r.status, location: r.location, err: /class="notification is-danger[^"]*"[^>]*>([^<]{0,90})/.exec(r.body)?.[1] || null });

(async () => {
  const out = {};

  // ---- 1. signup with a mixed-case username: what lands in the DB, what lands in the session
  const s1 = await req('POST', '/signup', {
    form: { username: 'ZetaUser', password: 'Passw0rd123', confirmPassword: 'Passw0rd123' },
    xff: '10.20.0.1',
  });
  out.signup_mixed_case = summary(s1);
  const cookie1 = sidOf(s1);
  out.signup_mixed_case.cookie = cookie1;
  if (cookie1) {
    const me = await req('GET', '/api/me', { cookie: cookie1, xff: '10.20.0.1' });
    out.session_api_me = JSON.parse(me.body);
  }

  // signup again with the same name in a different case (from a different limiter bucket)
  const s2 = await req('POST', '/signup', {
    form: { username: 'ZETAUSER', password: 'Passw0rd999', confirmPassword: 'Passw0rd999' },
    xff: '10.20.0.2',
  });
  out.signup_same_name_other_case = summary(s2);

  // login with the other case of an existing (normalized) account
  const l1 = await req('POST', '/login', { form: { username: 'ZETAUSER', password: 'Passw0rd123' }, xff: '10.20.0.3' });
  out.login_other_case_existing = summary(l1);

  // ---- 2. legacy mixed-case account vs new lowercase signup (collision takeover)
  // 'LegacyUser' is inserted into the lane DB by the harness just before this run.
  const s3 = await req('POST', '/signup', {
    form: { username: 'legacyuser', password: 'AttackerPass1', confirmPassword: 'AttackerPass1' },
    xff: '10.20.0.4',
  });
  out.signup_legacyuser_attacker = summary(s3);

  const l2 = await req('POST', '/login', { form: { username: 'LegacyUser', password: 'AttackerPass1' }, xff: '10.20.0.5' });
  out.login_as_LegacyUser_with_attacker_password = summary(l2);
  const cookieVictimName = sidOf(l2);
  if (cookieVictimName) {
    const me = await req('GET', '/api/me', { cookie: cookieVictimName, xff: '10.20.0.5' });
    out.who_am_i_after_that_login = JSON.parse(me.body);
  }

  const l3 = await req('POST', '/login', { form: { username: 'LegacyUser', password: 'VictimPass1' }, xff: '10.20.0.6' });
  out.login_as_LegacyUser_with_victim_password = summary(l3);

  const l4 = await req('POST', '/login', { form: { username: 'legacyuser', password: 'VictimPass1' }, xff: '10.20.0.7' });
  out.login_as_lowercase_legacy_with_victim_password = summary(l4);

  console.log(JSON.stringify(out, null, 1));
})();
