// Lane frame probe: drive the WebGL build in Edge over CDP (no external deps, plain
// WebSocket + JSON) and save canvas frames plus the console trail.
//
// Guest boot by default: the served page carries -AUTOSERVE (Title -> Login guest start ->
// StoryPlay skipped -> FirstCharaSelect) and no -REGULAR/-DATAID splice, so the client mints a
// fresh guest identity and reaches character select. The shipped player's AutoServe *does* decide
// that window, about 3 s after the scene loads (observed: arrival 94 s, decide 97 s, Home 97 s), so
// anything that has to land inside the window - a roster cell pick - goes in as an arrival frame or
// a post-arrival click, not after the settling passes.
//   node scripts/lane_frame_probe.cjs [url] [timeoutMs]
// env: TARGET_SCENE (FirstCharaSelect) FRAME_COUNT (6) SETTLE_MS (9000) HEADED (0)
//      OUT_DIR (C:\mfgtmp\lane_frames) LABEL (lane)
//      ARRIVAL_FRAMES (0) ARRIVAL_GAP_MS (400) POST_CLICK_PLAN POST_CLICK_DELAY_MS (0) POST_CLICK_ROUNDS (1) CLICK_PLAN
const fs = require('fs');
const path = require('path');
const os = require('os');
const { spawn } = require('child_process');

const URL0 = process.argv[2] || 'http://127.0.0.1:8094/aog/index.html?-MFGDBG';
const MS = parseInt(process.argv[3] || '240000', 10);
const TARGET = process.env.TARGET_SCENE || 'FirstCharaSelect';
const FRAMES = parseInt(process.env.FRAME_COUNT || '6', 10);
const SETTLE = parseInt(process.env.SETTLE_MS || '9000', 10);
const HEADED = process.env.HEADED === '1';
const OUT = process.env.OUT_DIR || 'C:\\mfgtmp\\lane_frames';
const LABEL = process.env.LABEL || 'lane';
const EDGE = process.env.EDGE_EXE || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe';

const t0 = Date.now();
const stamp = () => String(Math.round((Date.now() - t0) / 1000)).padStart(3, ' ');
fs.mkdirSync(OUT, { recursive: true });
const logFile = path.join(OUT, LABEL + '_console.log.txt');
fs.writeFileSync(logFile, '');
const log = (s) => { const line = `[${stamp()}s] ${s}`; console.log(line); fs.appendFileSync(logFile, line + '\n'); };

const udd = fs.mkdtempSync(path.join(os.tmpdir(), 'edge-frameprobe-'));
const flags = [
  '--no-sandbox', '--enable-features=SharedArrayBuffer', '--allow-cross-origin-authentication',
  '--window-size=1300,820', '--window-position=40,40', '--disable-background-timer-throttling',
  '--disable-renderer-backgrounding', '--disable-backgrounding-occluded-windows', '--disable-hang-monitor',
  '--autoplay-policy=no-user-gesture-required', '--ignore-gpu-blocklist', '--enable-unsafe-swiftshader',
  '--no-first-run', '--no-default-browser-check', `--user-data-dir=${udd}`, '--remote-debugging-port=0',
];
if (!HEADED) flags.unshift('--headless=new');
flags.push('about:blank');
const child = spawn(EDGE, flags, { detached: true, stdio: 'ignore' });
child.unref();
const spawnedPid = child.pid;

function cleanup() {
  // Only processes carrying this run's own user-data-dir (Edge hands off to a detached child).
  try {
    const out = require('child_process').execSync(
      `powershell -NoProfile -Command "Get-CimInstance Win32_Process -Filter \\"Name='msedge.exe'\\" | Where-Object { $_.CommandLine -like '*${udd}*' } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force }"`,
      { stdio: 'ignore' });
  } catch (e) { /* nothing matched */ }
  try { if (spawnedPid) process.kill(spawnedPid); } catch (e) { }
  fs.rmSync(udd, { recursive: true, force: true });
}

async function waitEndpoint() {
  const portFile = path.join(udd, 'DevToolsActivePort');
  for (let i = 0; i < 300; i++) {
    try {
      const [port, p] = fs.readFileSync(portFile, 'utf8').trim().split('\n').map((x) => x.trim());
      if (port && p) return { port, browserWs: `ws://127.0.0.1:${port}${p}` };
    } catch (e) { }
    await new Promise((r) => setTimeout(r, 100));
  }
  throw new Error('Edge did not publish DevToolsActivePort within 30s');
}

function cdpConnect(wsUrl) {
  return new Promise((resolve, reject) => {
    const ws = new WebSocket(wsUrl);
    const pending = new Map();
    const handlers = [];
    let nextId = 1;
    ws.onopen = () => resolve({
      close: () => { try { ws.close(); } catch (e) { } },
      on: (fn) => handlers.push(fn),
      send: (method, params = {}, sessionId) => new Promise((res, rej) => {
        const id = nextId++;
        pending.set(id, { res, rej, method });
        ws.send(JSON.stringify({ id, method, params, ...(sessionId ? { sessionId } : {}) }));
      }),
    });
    ws.onerror = (e) => reject(new Error('cdp socket error ' + (e.message || e.type)));
    ws.onmessage = (ev) => {
      const msg = JSON.parse(ev.data);
      if (msg.id && pending.has(msg.id)) {
        const p = pending.get(msg.id); pending.delete(msg.id);
        if (msg.error) p.rej(new Error(p.method + ': ' + msg.error.message)); else p.res(msg.result);
        return;
      }
      for (const h of handlers) { try { h(msg); } catch (e) { } }
    };
  });
}

const timed = (p, ms, tag) => Promise.race([
  p, new Promise((_, rej) => setTimeout(() => rej(new Error(tag + ' timeout ' + ms + 'ms')), ms)),
]);

(async () => {
  let cdp = null;
  let browser = null;
  let done = false;
  try {
    const { port, browserWs } = await waitEndpoint();
    browser = await timed(cdpConnect(browserWs), 20000, 'cdp connect');
    log('edge up pid=' + spawnedPid + ' profile=' + udd);

    const { targetInfos } = await timed(browser.send('Target.getTargets'), 15000, 'Target.getTargets');
    const pages = targetInfos.filter((t) => t.type === 'page');
    log('targets: ' + pages.map((t) => t.targetId.slice(0, 8) + '=' + t.url).join(' | '));
    let target = pages.find((t) => /about:blank/.test(t.url)) || pages[0];
    if (!target) {
      const created = await timed(browser.send('Target.createTarget', { url: 'about:blank' }), 15000, 'Target.createTarget');
      target = { targetId: created.targetId };
    }
    cdp = await timed(cdpConnect(`ws://127.0.0.1:${port}/devtools/page/${target.targetId}`), 20000, 'page connect');
    await cdp.send('Page.enable');
    await cdp.send('Runtime.enable');
    // Deterministic geometry (the WebGL template letterboxes to 16:9) and dpr 1 so canvas CSS
    // pixels map 1:1 to screenshot pixels across runs.
    await cdp.send('Emulation.setDeviceMetricsOverride', { width: 1280, height: 720, deviceScaleFactor: 1, mobile: false });
    // The Unity WebGL player grabs fullscreen on any canvas interaction, which rescales the canvas
    // and breaks fixed pixel coordinates; neuter it before the loader runs.
    await cdp.send('Page.addScriptToEvaluateOnNewDocument', {
      source: `window.__dsProbeNeuter = setInterval(function(){
        try {
          var u = window.UnityObject && window.UnityObject();
          if (u && u.Module) { u.Module.SetFullscreen = function(){}; u.Module.toggleFullscreen = function(){}; }
          if (document.fullscreenElement) { document.exitFullscreen().catch(function(){}); }
        } catch (e) {}
      }, 500);`,
    });

    const scenes = [];
    const consoleLines = [];
    cdp.on((msg) => {
      if (msg.method === 'Runtime.consoleAPICalled') {
        const text = (msg.params.args || []).map((a) => a.value ?? a.description ?? '').join(' ');
        if (!text) return;
        consoleLines.push(text);
        if (consoleLines.length % 40 === 0 || /MFGDBG|AUTOSERVE|LoadScene|Exception/.test(text)) {
          // The dump lines are long by design - a whole material's bound set on one line - so they get a
          // wider slice than the rest of the console trail. At 180 chars the ramp, tint and gain values,
          // which are the proof a knob landed, were cut off.
          log('console: ' + text.slice(0, /MFGDBG (MATDUMP|LIGHTDUMP|CAMDUMP)/.test(text) ? 760 : 180));
        }
        const m = /active='([^']+)'/.exec(text);
        if (m && !scenes.includes(m[1])) { scenes.push(m[1]); log('scene: ' + m[1]); }
        if (/LoadScene|SceneManager/.test(text) && text.includes(TARGET) && !scenes.includes(TARGET)) {
          scenes.push(TARGET); log('scene: ' + TARGET + ' (load log)');
        }
      }
    });

    log('goto ' + URL0);
    await timed(cdp.send('Page.navigate', { url: URL0 }), 30000, 'navigate');

    // Supplementary pointer clicks at canvas fractions (CLICK_PLAN="Scene:fx,fy;Scene:fx,fy").
    // Used for the story skip-all confirm, which AutoServe's rules reach but do not confirm.
    const plan = (process.env.CLICK_PLAN || '').split(';').map((s) => s.trim()).filter(Boolean).map((s) => {
      const [scene, xy] = s.split(':');
      const [fx, fy] = xy.split(',').map(Number);
      return { scene, fx, fy, last: 0 };
    });
    const clickCooldown = parseInt(process.env.CLICK_COOLDOWN_MS || '2500', 10);
    let rect = null; let rectAt = 0;
    const getRect = async () => {
      if (rect && Date.now() - rectAt < 4000) return rect;
      try {
        const r = await timed(cdp.send('Runtime.evaluate', {
          expression: `(() => { const c = document.querySelector('canvas'); if (!c) return ''; const b = c.getBoundingClientRect();
            return JSON.stringify({ x: Math.round(b.x), y: Math.round(b.y), w: Math.round(b.width), h: Math.round(b.height) }); })()`,
          returnByValue: true,
        }), 8000, 'rect');
        if (r.result.value) { rect = JSON.parse(r.result.value); rectAt = Date.now(); }
      } catch (e) { }
      return rect;
    };
    const dispatchClick = async (fx, fy) => {
      const r = await getRect(); if (!r) return;
      const x = r.x + fx * r.w, y = r.y + fy * r.h;
      for (const type of ['mouseMoved', 'mousePressed', 'mouseReleased']) {
        await timed(cdp.send('Input.dispatchMouseEvent', {
          type, x, y, button: 'left', buttons: type === 'mousePressed' ? 1 : 0, clickCount: 1,
        }), 6000, 'click');
      }
      log('click ' + fx.toFixed(3) + ',' + fy.toFixed(3) + ' -> page px ' + Math.round(x) + ',' + Math.round(y));
    };

    const deadline = Date.now() + MS;
    while (Date.now() < deadline && !scenes.includes(TARGET)) {
      await new Promise((r) => setTimeout(r, 500));
      const active = scenes[scenes.length - 1] || '';
      for (const p of plan) {
        if (p.scene !== active) continue;
        if (Date.now() - p.last < clickCooldown) continue;
        p.last = Date.now();
        try { await dispatchClick(p.fx, p.fy); } catch (e) { log('click failed: ' + e.message); }
      }
    }
    if (!scenes.includes(TARGET)) {
      log('target scene ' + TARGET + ' never reached; scenes seen: ' + scenes.join(','));
    }

    // Screenshots the moment the target scene appears, before any settling (ARRIVAL_FRAMES, gap
    // ARRIVAL_GAP_MS). Character select is decided by the shipped harness about 3 s after the scene
    // loads, so a roster cell can only be seen - or picked - in the first frames after arrival; the
    // settling and page-info passes together cost more than the window lasts.
    const arrivalFrames = parseInt(process.env.ARRIVAL_FRAMES || '0', 10);
    const arrivalGap = parseInt(process.env.ARRIVAL_GAP_MS || '400', 10);
    for (let i = 0; i < arrivalFrames; i++) {
      const fn = path.join(OUT, `${LABEL}_${TARGET}_arrive_${String(i).padStart(2, '0')}.png`);
      try {
        const r = await getRect();
        const clip = r ? { x: r.x, y: r.y, width: r.w, height: r.h, scale: 1 } : undefined;
        const shot = await timed(cdp.send('Page.captureScreenshot', { format: 'png', clip }), 12000, 'screenshot');
        fs.writeFileSync(fn, Buffer.from(shot.data, 'base64'));
        log('arrive frame ' + fn + ' ' + fs.statSync(fn).size + ' bytes');
      } catch (e) { log('arrive frame failed: ' + e.message); }
      await new Promise((r) => setTimeout(r, arrivalGap));
    }

    // Post-arrival clicks (POST_CLICK_PLAN="fx,fy;fx,fy") repeated POST_CLICK_ROUNDS times:
    // navigate within the target scene, e.g. pick a roster cell on character select.
    const postPlan = (process.env.POST_CLICK_PLAN || '').split(';').map((s) => s.trim()).filter(Boolean).map((s) => {
      const [fx, fy] = s.split(',').map(Number);
      return { fx, fy };
    });
    const rounds = parseInt(process.env.POST_CLICK_ROUNDS || '1', 10);
    // POST_CLICK_DELAY_MS: the roster slides in, so a click has to land after it settles and before the
    // shipped AutoServe's next decide poll. The arrival frames above also cost wall time; the delay is
    // counted from the moment the target scene was first seen.
    const postDelay = parseInt(process.env.POST_CLICK_DELAY_MS || '0', 10);
    if (postDelay > 0) {
      log('post click delay ' + postDelay + 'ms');
      await new Promise((r) => setTimeout(r, postDelay));
    }
    for (let round = 0; round < rounds; round++) {
      for (const p of postPlan) {
        try { await dispatchClick(p.fx, p.fy); } catch (e) { log('post click failed: ' + e.message); }
        await new Promise((r) => setTimeout(r, 900));
      }
      if (postPlan.length) await new Promise((r) => setTimeout(r, clickCooldown));
    }
    if (scenes.includes(TARGET)) {
      log('settling ' + SETTLE + 'ms on ' + TARGET);
      await new Promise((r) => setTimeout(r, SETTLE));
    }

    const info = await timed(cdp.send('Runtime.evaluate', {
      expression: `(() => {
        const c = document.querySelector('canvas');
        const r = c ? c.getBoundingClientRect() : null;
        let gl = null, renderer = '', version = '';
        try {
          gl = c && (c.getContext('webgl2', { preserveDrawingBuffer: false }) || c.getContext('webgl'));
          if (gl) {
            const dbg = gl.getExtension('WEBGL_debug_renderer_info');
            renderer = dbg ? String(gl.getParameter(dbg.UNMASKED_RENDERER_WEBGL)) : String(gl.getParameter(gl.RENDERER));
            version = String(gl.getParameter(gl.VERSION));
          }
        } catch (e) { renderer = 'probe-failed: ' + e.message; }
        return JSON.stringify({
          canvas: r ? { x: Math.round(r.x), y: Math.round(r.y), w: Math.round(r.width), h: Math.round(r.height) } : null,
          buffer: c ? { w: c.width, h: c.height } : null,
          dpr: window.devicePixelRatio, renderer, version,
          title: document.title, unity: typeof window.UnityObject !== 'undefined' || typeof window.createUnityInstance !== 'undefined',
          progress: (document.querySelector('#unity-loading-bar') || {}).style ? document.querySelector('#unity-loading-bar').style.display : 'n/a',
        });
      })()`, returnByValue: true,
    }), 15000, 'evaluate');
    log('page ' + info.result.value);
    const page = JSON.parse(info.result.value);

    fs.writeFileSync(path.join(OUT, LABEL + '_page.json'), JSON.stringify({ url: URL0, scenes, page }, null, 2));
    fs.writeFileSync(path.join(OUT, LABEL + '_console_dump.txt'), consoleLines.join('\n'));

    const sceneTag = scenes.includes(TARGET) ? TARGET : (scenes[scenes.length - 1] || 'noscene');
    for (let i = 0; i < FRAMES; i++) {
      const fn = path.join(OUT, `${LABEL}_${sceneTag}_${String(i).padStart(2, '0')}.png`);
      try {
        const clip = page.canvas
          ? { x: page.canvas.x, y: page.canvas.y, width: page.canvas.w, height: page.canvas.h, scale: 1 }
          : undefined;
        const shot = await timed(cdp.send('Page.captureScreenshot', { format: 'png', clip }), 12000, 'screenshot');
        fs.writeFileSync(fn, Buffer.from(shot.data, 'base64'));
        log('frame ' + fn + ' ' + fs.statSync(fn).size + ' bytes');
      } catch (e) {
        log('frame ' + i + ' failed: ' + e.message);
      }
      await new Promise((r) => setTimeout(r, 1200));
    }
    done = true;
  } catch (e) {
    log('FAILED ' + e.message);
  } finally {
    try { if (cdp) cdp.close(); } catch (e) { }
    cleanup();
    log('browser closed, profile removed; frames=' + done);
  }
  process.exit(0);
})();
