const http = require('http');
function req(method, path, form, xff) {
  return new Promise((resolve) => {
    const body = new URLSearchParams(form).toString();
    const r = http.request({ host: '127.0.0.1', port: 8191, path, method, headers: {
      'Content-Type': 'application/x-www-form-urlencoded', 'Content-Length': Buffer.byteLength(body),
      'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) Chrome/120 Safari/537.36', 'Accept': 'text/html',
      'X-Forwarded-For': xff } }, (res) => {
      let d = ''; res.on('data', (c) => (d += c));
      res.on('end', () => resolve({ status: res.statusCode, location: res.headers.location, cookie: (res.headers['set-cookie'] || []).join('|') }));
    });
    r.end(body);
  });
}
const get = (path, cookie, xff) => new Promise((resolve) => {
  const r = http.request({ host: '127.0.0.1', port: 8191, path, headers: { 'User-Agent': 'Mozilla/5.0 Chrome/120', 'Accept': 'application/json', Cookie: cookie, 'X-Forwarded-For': xff } }, (res) => {
    let d = ''; res.on('data', (c) => (d += c)); res.on('end', () => resolve({ status: res.statusCode, body: d }));
  });
  r.end();
});
(async () => {
  const owner = await req('POST', '/login', { username: 'ClaimUser', password: 'OwnerPass123' }, '10.40.0.1');
  const intruder = await req('POST', '/login', { username: 'ClaimUser', password: 'Intruder123' }, '10.40.0.2');
  const sid = (intruder.cookie.match(/connect\.sid=[^;]+/) || [null])[0];
  const me = sid ? await get('/api/me', sid, '10.40.0.2') : null;
  console.log(JSON.stringify({
    owner_login: { status: owner.status, location: owner.location },
    intruder_login_using_owner_name: { status: intruder.status, location: intruder.location },
    intruder_session_identity: me ? me.body : null,
  }, null, 1));
})();
