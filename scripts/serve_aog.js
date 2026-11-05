// Static front for the freshly assembled WebGL webroot, mounted at /aog (capture-lane webfront).
// Serves C:\mfgtmp\webroot under http://127.0.0.1:WP/aog/... with COOP/COEP headers for
// crossOriginIsolated (SharedArrayBuffer), brotli sidecars (.br), and single-range support.
const http = require('http');
const fs = require('fs');
const path = require('path');

const ROOT = path.normalize(process.env.WEBFRONT_ROOT || 'C:\\mfgtmp\\webroot');
const PORT = parseInt(process.env.WEBFRONT_PORT || '8094', 10);
const MOUNT = '/aog';

const MIME = {
  '.html': 'text/html; charset=utf-8', '.js': 'text/javascript', '.wasm': 'application/wasm',
  '.json': 'application/json', '.css': 'text/css', '.bundle': 'application/octet-stream',
  '.webm': 'video/webm', '.ogg': 'audio/ogg', '.data': 'application/octet-stream',
  '.ico': 'image/x-icon', '.png': 'image/png', '.jpg': 'image/jpeg', '.txt': 'text/plain',
};
function mimeOf(p) {
  let f = p.toLowerCase();
  if (f.endsWith('.br')) f = f.slice(0, -3);
  return MIME[path.extname(f)] || 'application/octet-stream';
}

http.createServer((req, res) => {
  try {
    if (req.method !== 'GET' && req.method !== 'HEAD') {
      res.writeHead(405, { Allow: 'GET, HEAD' }); res.end(); return;
    }
    const url = decodeURIComponent(req.url.split('?')[0]);
    if (url !== MOUNT && !url.startsWith(MOUNT + '/')) {
      res.writeHead(404, { 'Content-Type': 'text/plain' }); res.end('not mounted'); return;
    }
    let rel = url.slice(MOUNT.length) || '/index.html';
    const file = path.normalize(path.join(ROOT, rel.replace(/\//g, path.sep)));
    if (file !== ROOT && !file.startsWith(ROOT + path.sep)) {
      res.writeHead(403, { 'Content-Type': 'text/plain' }); res.end('forbidden'); return;
    }
    let f = file, enc = null;
    const ae = req.headers['accept-encoding'] || '';
    if (/(^|,)\s*br\b/.test(ae) && fs.existsSync(f + '.br')) { f = f + '.br'; enc = 'br'; }
    if (!fs.existsSync(f) || !fs.statSync(f).isFile()) {
      res.writeHead(404, { 'Content-Type': 'text/plain' }); res.end('not found'); return;
    }
    const st = fs.statSync(f);
    const h = {
      'Content-Type': mimeOf(file),
      'Cross-Origin-Opener-Policy': 'same-origin',
      'Cross-Origin-Embedder-Policy': 'require-corp',
      'Access-Control-Allow-Origin': '*',
      'Cache-Control': 'no-store',
      'Accept-Ranges': 'bytes',
    };
    if (enc) h['Content-Encoding'] = 'br';
    const rm = (!enc && req.headers['range']) ? /^bytes=(\d*)-(\d*)$/.exec(req.headers['range']) : null;
    if (rm && (rm[1] || rm[2])) {
      let s, e;
      if (rm[1]) {
        s = parseInt(rm[1], 10);
        e = rm[2] ? Math.min(parseInt(rm[2], 10), st.size - 1) : st.size - 1;
      } else {
        s = Math.max(0, st.size - parseInt(rm[2], 10)); e = st.size - 1;
      }
      if (isNaN(s) || isNaN(e) || s > e) {
        res.writeHead(416, { 'Content-Range': 'bytes */' + st.size }); res.end(); return;
      }
      h['Content-Range'] = 'bytes ' + s + '-' + e + '/' + st.size;
      h['Content-Length'] = String(e - s + 1);
      res.writeHead(206, 'Partial Content', h);
      if (req.method === 'HEAD') { res.end(); return; }
      fs.createReadStream(f, { start: s, end: e }).pipe(res);
      return;
    }
    h['Content-Length'] = String(st.size);
    res.writeHead(200, 'OK', h);
    if (req.method === 'HEAD' || st.size === 0) { res.end(); return; }
    fs.createReadStream(f).pipe(res);
  } catch (e) {
    try { res.writeHead(500, { 'Content-Type': 'text/plain' }); res.end('err ' + e.message); } catch (_) { }
  }
}).listen(PORT, '127.0.0.1', () => console.log('serve_aog listening on http://127.0.0.1:' + PORT + MOUNT + '/ root=' + ROOT));
