const http = require('http');
function post(path, form, xff) {
  return new Promise((resolve) => {
    const body = new URLSearchParams(form).toString();
    const r = http.request({ host: '127.0.0.1', port: 8191, path, method: 'POST', headers: {
      'Content-Type': 'application/x-www-form-urlencoded', 'Content-Length': Buffer.byteLength(body),
      'User-Agent': 'Mozilla/5.0 Chrome/120', 'Accept': 'text/html', 'X-Forwarded-For': xff } }, (res) => {
      let d = ''; res.on('data', (c) => (d += c));
      res.on('end', () => resolve({ status: res.statusCode, location: res.headers.location, cookie: (res.headers['set-cookie'] || []).map(c=>c.split(';')[0]).join('|'), body: d }));
    });
    r.end(body);
  });
}
const get = (path, cookie, xff) => new Promise((resolve) => {
  const r = http.request({ host: '127.0.0.1', port: 8191, path, headers: { 'User-Agent': 'Mozilla/5.0 Chrome/120', 'Accept': 'text/html', Cookie: cookie, 'X-Forwarded-For': xff } }, (res) => {
    let d = ''; res.on('data', (c) => (d += c)); res.on('end', () => resolve({ status: res.statusCode, body: d }));
  });
  r.end();
});
(async () => {
  const s = await post('/signup', { username: 'WobbleUser', password: 'Wobble1234', confirmPassword: 'Wobble1234' }, '10.50.0.1');
  const sid = (s.cookie.match(/connect\.sid=[^;]+/) || [null])[0];
  const about = sid ? await get('/about', sid, '10.50.0.1') : null;
  const names = about ? [...new Set((about.body.match(/[Ww]obble[Uu]ser/g) || []))] : null;
  console.log(JSON.stringify({ signup_status: s.status, location: s.location, about_status: about && about.status, name_variants_shown_on_about: names }, null, 1));
})();
