// Reproduce the core's limiter/trust-proxy shape with the core's own express +
// express-rate-limit versions: app.set('trust proxy', 1), single-hop
// X-Forwarded-For written by the gate, socket peer 127.0.0.1.
const express = require('C:/mfg/tmp/RyuNET-core/node_modules/express');
const rateLimit = require('C:/mfg/tmp/RyuNET-core/node_modules/express-rate-limit');

const app = express();
app.set('trust proxy', 1);
const authLimit = rateLimit({ windowMs: 15 * 60 * 1000, max: 10, message: 'Too many login/signup attempts from this IP, please try again after 15 minutes' });
app.get('/ip', (req, res) => res.json({ ip: req.ip, ips: req.ips, socket: req.socket.remoteAddress }));
app.post('/login', authLimit, (req, res) => res.json({ ok: true, countedAs: req.ip }));

const server = app.listen(8123, '127.0.0.1', async () => {
  const http = require('http');
  const call = (path, xff, method) => new Promise((resolve) => {
    const req = http.request({ host: '127.0.0.1', port: 8123, path, method, headers: xff ? { 'X-Forwarded-For': xff } : {} }, (res) => {
      let d = '';
      res.on('data', (c) => (d += c));
      res.on('end', () => resolve({ status: res.statusCode, body: d.slice(0, 120) }));
    });
    req.end();
  });

  for (const xff of ['203.0.113.7', '198.51.100.9', '203.0.113.7, 127.0.0.1']) {
    const r = await call('/ip', xff, 'GET');
    console.log('GET /ip  XFF=' + xff + ' -> ' + r.body);
  }
  const r0 = await call('/ip', null, 'GET');
  console.log('GET /ip  no XFF       -> ' + r0.body);

  // 12 POSTs /login from 12 distinct spoofed visitor IPs, max is 10 per bucket.
  const codes = [];
  for (let i = 1; i <= 12; i++) {
    const r = await call('/login', '203.0.113.' + i, 'POST');
    codes.push(r.status);
  }
  console.log('POST /login from 12 distinct XFF IPs -> statuses: ' + codes.join(','));
  const r2 = await call('/login', '203.0.113.99', 'POST');
  console.log('POST /login fresh spoofed IP 203.0.113.99 -> ' + r2.status + ' ' + r2.body);
  server.close();
});
