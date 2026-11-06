#!/usr/bin/env bash
# assemble_webroot.sh - bash port of scripts/assemble_webroot.ps1 (147 lines) for a Linux host.
#
# Mirrors a Unity WebGL build tree into the lane webroot, patches it the same way the
# PowerShell script does, stamps the build asset URLs, and (optionally) runs the brotli
# sidecar tool. Step order and stdout messages match the PowerShell original.
#
#   1. require the build tree to exist                         (ps1 :25)
#   2. stop only the gate pid this lane recorded               (ps1 :29-36)
#   3. wipe the webroot, mirror the build tree into it         (ps1 :37-38)
#   4. bake config.arguments merge function into index.html    (ps1 :39-60)
#   5. canvas 960x600 -> 1280x720 plus letterbox-fill CSS      (ps1 :61-75)
#   6. framework.js URL-query -> argv rewrite                  (ps1 :76-90)
#   7. hash stamp over the four Build artifacts                (ps1 :91-104)
#   8. build_stamp.txt, catalog XHR stamp, refresh watchdog    (ps1 :105-131)
#   9. brotli sidecars                                         (ps1 :132-135)
#  10. optional webfront serve                                 (ps1 :136-147)
#
# Usage
# -----
#   scripts/linux/assemble_webroot.sh [--no-gate-stop] [--no-brotli] [--serve] [-h|--help]
#
#   Everything the PowerShell version took as a parameter is taken from the
#   environment here; the three switches map to the three flags above.
#
#   BUILD_DIR       Build tree to mirror.        default: ~/mfg/out/webgl
#                   (Unity WebGL output: index.html, Build/, StreamingAssets/, TemplateData/)
#   WEBROOT         Assembled webroot.           default: ~/mfgtmp/webroot
#                   Wiped (rm -rf) and recreated from BUILD_DIR on every run.
#   UPSTREAM        Identity origin URL.         default: http://127.0.0.1:8080
#                   Only used by --serve (WEBFRONT_UPSTREAM).
#   FRONT_URL       URL baked into the player args as the -FRONTURL value.
#                                                  default: http://127.0.0.1:8090/aog
#                   The gate rewrites this value positionally at serve time
#                   (webfront/Program.cs:1384), so any literal is fine as long as
#                   -FRONTURL stays its own array element.
#   BIND            'any' (all interfaces, guest VMs can load the page) or 'loopback'.
#                                                  default: any
#                   Only used by --serve (WEBFRONT_BIND).
#   PID_DIR         Lane pid directory - the one lane_up.sh:88 and lane_down.sh:50 use.
#                                                  default: ~/mfgtmp/lane_pids
#   GATE_PID_FILE   File holding the pid of the gate this lane owns.
#                                                  default: $PID_DIR/gate.pid
#                   ONE recorded gate pid: lane_up.sh:427 writes exactly this file,
#                   lane_down.sh:209 stops it, and --serve here records the gate it
#                   starts into the same file. The old default (~/mfgtmp/gate_pid.txt)
#                   was read by this script and assemble_webroot.ps1:29 and written by
#                   nothing in the repo, so the stop step was a guaranteed no-op for a
#                   lane_up.sh lane.
#                   If the file is absent, nothing is signalled. A pid is signalled only
#                   when its command line names the gate program AND its working
#                   directory is the gate's own bin dir - the cwd rule lane_up.sh:171-182
#                   and lane_down.sh:120-131 use. When a live recorded gate cannot be
#                   stopped, the webroot wipe is refused instead of run under it.
#   GATE_PROCESS    Process name the recorded pid must match before any signal.
#                                                  default: WebFront
#                   Matches comm, argv[0] basename or argv[1] basename (dotnet
#                   WebFront.dll form), case-insensitive, .exe/.dll suffix ignored,
#                   exact match only (WebFrontTools.exe is not WebFront).
#   NO_GATE_STOP    1 = leave every gate process alone (probe assembly), = -NoGateStop.
#                                                  default: 0
#   NO_BROTLI       1 = skip the brotli sidecar pass, = -NoSidecar.  default: 0
#   BROTLI_QUALITY  Passed to the sidecar tool; the tool's own default is 6.  default: 6
#   SIDECAR_DLL     Sidecar tool assembly.
#                   default: <repo>/scripts/brotli-sidecar/bin/Release/net8.0/sidecar.dll
#                   Run as 'dotnet <SIDECAR_DLL> <webroot> <quality>' - the tool is
#                   net8.0 and framework-dependent, so the .dll runs on Linux where
#                   the .exe apphost does not. Needs a dotnet 8 runtime on PATH.
#   WEBFRONT_DLL    Gate/relay assembly, only used by --serve.
#                   default: <repo>/webfront/bin/Release/net8.0/WebFront.dll
#                   (AssemblyName is WebFront, so the assembly is named that on Linux too)
#   SERVE           1 = start webfront after assembling, = -Serve.   default: 0
#
# Portability notes
# -----------------
#   * bash only (set -o pipefail, ${BASH_SOURCE[0]}); the text surgery itself is plain
#     POSIX awk/sed/grep/tr/cut.
#   * GNU/coreutils features used, each noted at its call site: cp -a (POSIX fallback
#     cp -R), sha256sum (POSIX fallback openssl dgst -sha256), head -c (POSIX fallback
#     cut -b 1-N).
#   * No GNU-only regex tricks: every patch is a literal (non-regex) search/replace,
#     matching .NET String.Replace and the fixed strings the PowerShell -replace
#     patterns were matching. awk index()/substr() is used so the needles' " / \ & ( )
#     characters never need escaping.
#   * LC_ALL=C keeps the byte stream byte-exact (the build artifacts are pure ASCII).

set -euo pipefail
export LC_ALL=C

# ---------------------------- configuration ----------------------------
BUILD_DIR=${BUILD_DIR:-"$HOME/mfg/out/webgl"}
WEBROOT=${WEBROOT:-"$HOME/mfgtmp/webroot"}
UPSTREAM=${UPSTREAM:-'http://127.0.0.1:8080'}
FRONT_URL=${FRONT_URL:-'http://127.0.0.1:8090/aog'}
BIND=${BIND:-'any'}
PID_DIR=${PID_DIR:-"$HOME/mfgtmp/lane_pids"}           # same default as lane_up.sh:88
GATE_PID_FILE=${GATE_PID_FILE:-"$PID_DIR/gate.pid"}   # the file lane_up.sh:427 writes
GATE_PROCESS=${GATE_PROCESS:-'WebFront'}
NO_GATE_STOP=${NO_GATE_STOP:-0}
NO_BROTLI=${NO_BROTLI:-0}
BROTLI_QUALITY=${BROTLI_QUALITY:-6}
SERVE=${SERVE:-0}

# ps1 :23 - $root = parent of the parent of the script path; the sidecar tool is found
# relative to the repo root, not relative to the caller's cwd. BASH_SOURCE is bash's
# stand-in for $MyInvocation.MyCommand.Path.
SCRIPT_PATH=${BASH_SOURCE[0]}
SCRIPT_DIR=${SCRIPT_PATH%/*}                 # <repo>/scripts/linux
REPO_ROOT=${SCRIPT_DIR%/*}                   # <repo>/scripts
REPO_ROOT=${REPO_ROOT%/*}                    # <repo>
SIDECAR_DLL=${SIDECAR_DLL:-"$REPO_ROOT/scripts/brotli-sidecar/bin/Release/net8.0/sidecar.dll"}
WEBFRONT_DLL=${WEBFRONT_DLL:-"$REPO_ROOT/webfront/bin/Release/net8.0/WebFront.dll"}

usage() {
  sed -n '2,/^set -euo/p' "$SCRIPT_PATH" | sed '$d'
}

die() { printf 'assemble_webroot: %s\n' "$1" >&2; exit 1; }

while [ $# -gt 0 ]; do
  case $1 in
    --no-gate-stop) NO_GATE_STOP=1 ;;
    --no-brotli)    NO_BROTLI=1 ;;
    --serve)        SERVE=1 ;;
    -h|--help)      usage; exit 0 ;;
    *)              die "unknown argument: $1" ;;
  esac
  shift
done

# ---------------------------- helpers ----------------------------

# litreplace <file> <needle> <replacement> <all|first> <out>
# Literal (not regex) search/replace, whole file, line by line. The needle and the
# replacement travel through the environment on purpose: awk -v would interpret
# backslashes in them, ENVIRON[] does not. 'all' mirrors .NET String.Replace (replaces
# every occurrence, which is what the PowerShell version used); 'first' mirrors the
# [regex]::Replace(..., 1) call at ps1 :86.
litreplace() {
  _lr_needle=$2 _lr_repl=$3 _lr_mode=$4 awk '
    BEGIN { needle = ENVIRON["_lr_needle"]; repl = ENVIRON["_lr_repl"]; mode = ENVIRON["_lr_mode"] }
    {
      out = ""; s = $0
      for (;;) {
        i = index(s, needle)
        if (i == 0) break
        out = out substr(s, 1, i - 1) repl
        s = substr(s, i + length(needle))
        if (mode == "first") break
      }
      print out s
    }
  ' "$1" > "$5"
}

# apply_patch <file> <needle> <replacement> <all|first>
# Writes the patched text back in place, preserving the file's trailing-newline state
# (Set-Content -NoNewline wrote the string back exactly as Get-Content -Raw read it).
apply_patch() {
  _p_file=$1 _p_needle=$2 _p_repl=$3 _p_mode=$4
  _p_tmp=${_p_file}.lit.tmp
  litreplace "$_p_file" "$_p_needle" "$_p_repl" "$_p_mode" "$_p_tmp"
  if [ -n "$(tail -c 1 "$_p_file")" ]; then
    # input ended without a newline; awk's print added one, so drop it.
    # head -c is a coreutils/BSD extension; POSIX alternative: cut -b 1-N.
    _p_size=$(wc -c < "$_p_tmp")
    head -c "$((_p_size - 1))" "$_p_tmp" > "$_p_file"
  else
    cat "$_p_tmp" > "$_p_file"
  fi
  rm -f -- "$_p_tmp"
}

has() { grep -F -q -- "$2" "$1"; }   # has <file> <literal>

# SHA-256 hex, uppercase, over raw bytes. sha256sum is coreutils; openssl is the
# fallback. PowerShell used Get-FileHash -Algorithm SHA256 plus
# BitConverter.ToString (ps1 :97-99), which emits UPPER-CASE hex - the webroot's
# build_stamp.txt ("F5FACEA75F49") shows that casing, so the bash port upper-cases too.
if command -v sha256sum >/dev/null 2>&1; then
  _hash_stdin() { sha256sum | cut -d' ' -f1; }
elif command -v openssl >/dev/null 2>&1; then
  _hash_stdin() { openssl dgst -r -sha256 | cut -d' ' -f1; }
else
  die 'no sha256sum (coreutils) and no openssl on PATH'
fi
hash_upper() { _hash_stdin < "$1" | tr '[:lower:]' '[:upper:]'; }

_norm_name() { printf '%s' "$1" | tr 'A-Z' 'a-z' | sed 's/\.exe$//; s/\.dll$//'; }

# ---------------------------- 1. build tree ----------------------------
# ps1 :25  if (-not (Test-Path $Build)) { throw "no webgl build at $Build" }
if [ ! -d "$BUILD_DIR" ]; then die "no webgl build at $BUILD_DIR"; fi
case $WEBROOT in ''|'/') die "refusing to wipe WEBROOT=$WEBROOT" ;; esac

# ---------------------------- 2. stop the recorded gate ----------------------------
# ps1 :26-36. The PowerShell version read the pid out of gate_pid.txt and stopped that
# one process only after Get-CimInstance confirmed its name was WebFront.exe.
# Linux equivalent: same pid file, same single pid, verified through /proc/<pid> before
# any signal. Never a name kill, never pkill/pgrep, never a pattern, never a port owner:
# if the pid does not match the gate, nothing is signalled. If the file is absent,
# nothing happens at all.
pid_alive() { [ -e "/proc/$1" ] || kill -0 "$1" 2>/dev/null; }   # lane_down.sh:112 rule

stop_recorded_gate() {
  if [ "$NO_GATE_STOP" = 1 ]; then
    printf 'gate stop skipped (NO_GATE_STOP=1)\n'
    return 0
  fi
  if [ ! -f "$GATE_PID_FILE" ]; then
    return 0                                   # no pid file: do nothing (ps1 :30)
  fi
  # Get-Content -Raw + .Trim() (ps1 :32): first line, leading/trailing blanks dropped.
  # Anything else (inner spaces, text) fails the digit test below and nothing is killed.
  pid=$(sed -n '1p' "$GATE_PID_FILE" 2>/dev/null | tr -d '\r\t' | sed 's/^ *//; s/ *$//') || pid=''
  case $pid in
    ''|*[!0-9]*) printf 'gate pid file %s has no usable pid; nothing signalled\n' "$GATE_PID_FILE"; return 0 ;;
  esac                                       # [int]::TryParse failure (ps1 :32)
  if [ "$pid" -le 1 ]; then
    printf 'gate pid file holds pid %s; refusing to signal it\n' "$pid"; return 0
  fi
  if [ "$pid" = "$$" ] || [ "$pid" = "$PPID" ]; then
    printf 'gate pid file holds our own pid %s; refusing to signal it\n' "$pid"; return 0
  fi
  if ! pid_alive "$pid"; then
    printf 'gate pid %s is not running; stale pid file %s removed\n' "$pid" "$GATE_PID_FILE"
    rm -f -- "$GATE_PID_FILE"
    return 0
  fi
  if [ ! -r "/proc/$pid/cmdline" ]; then
    printf 'gate pid %s is alive but its command line cannot be read; nothing signalled\n' "$pid"
    return 1                                 # no identity proof: the wipe below must not run under it
  fi
  argv0=$(tr '\0' '\n' < "/proc/$pid/cmdline" | sed -n '1p') || argv0=''
  argv1=$(tr '\0' '\n' < "/proc/$pid/cmdline" | sed -n '2p') || argv1=''
  comm=$(sed -n '1p' "/proc/$pid/comm" 2>/dev/null) || comm=''
  want=$(_norm_name "$GATE_PROCESS")
  got_comm=$(_norm_name "$comm")
  got_exe=$(_norm_name "${argv0##*/}")
  got_arg=$(_norm_name "${argv1##*/}")       # dotnet WebFront.dll form
  if [ "$got_comm" != "$want" ] && [ "$got_exe" != "$want" ] && [ "$got_arg" != "$want" ]; then
    printf 'gate pid %s is "%s", not "%s"; not signalled\n' "$pid" "${got_exe:-$got_comm}" "$GATE_PROCESS"
    return 1                                 # never kill a pid that does not match
  fi
  # Identity rule two (lane_up.sh:171-182, lane_down.sh:120-131): the gate this lane owns
  # runs with its cwd at the bin dir of the assembly named by WEBFRONT_DLL. A recycled pid
  # that merely carries a gate-looking name is somebody else's process.
  gate_bin_dir=$(readlink -f "${WEBFRONT_DLL%/*}" 2>/dev/null) || gate_bin_dir=''
  pid_cwd=$(readlink -f "/proc/$pid/cwd" 2>/dev/null) || pid_cwd=''
  if [ -n "$gate_bin_dir" ] && [ "$pid_cwd" != "$gate_bin_dir" ]; then
    printf 'gate pid %s names the gate program but its working directory is %s, not %s: not signalled (not the gate this lane started)\n' "$pid" "${pid_cwd:-<unreadable>}" "$gate_bin_dir"
    return 1
  fi
  kill -TERM "$pid" 2>/dev/null || true      # let the gate close its listener before Force
  _st=1
  while [ $_st -le 10 ]; do pid_alive "$pid" || break; sleep 0.5; _st=$((_st+1)); done
  if pid_alive "$pid"; then
    kill -KILL "$pid" 2>/dev/null || true    # Stop-Process -Id <pid> -Force (ps1 :34)
    _st=1
    while [ $_st -le 10 ]; do pid_alive "$pid" || break; sleep 0.5; _st=$((_st+1)); done
  fi
  if pid_alive "$pid"; then
    printf 'gate pid %s survived SIGTERM and SIGKILL; refusing to wipe %s under a serving gate\n' "$pid" "$WEBROOT"
    return 1
  fi
  printf 'stopped gate pid=%s (%s); pid file %s cleared\n' "$pid" "${got_exe:-$got_comm}" "$GATE_PID_FILE"
  rm -f -- "$GATE_PID_FILE"
  return 0
}
if ! stop_recorded_gate; then
  die "the recorded gate ($GATE_PID_FILE) is live and was not signalled: refusing to wipe $WEBROOT under a serving gate. Stop that gate first (scripts/linux/lane_down.sh, or systemctl stop mfg-gate.service) and run again, or pass --no-gate-stop for a probe assembly."
fi

# ---------------------------- 3. wipe + mirror ----------------------------
# ps1 :37-38  Remove-Item $Webroot -Recurse -Force ; Copy-Item $Build $Webroot -Recurse
if [ -e "$WEBROOT" ]; then rm -rf -- "$WEBROOT"; fi
mkdir -p -- "$WEBROOT"
# cp -a is coreutils (mode+timestamps preserved, symlinks copied as links); POSIX
# fallback is cp -R. Copy-Item copied attributes too and did not follow symlinks.
cp -a "$BUILD_DIR"/. "$WEBROOT"/

# ---------------------------- 4. config.arguments merge ----------------------------
# ps1 :43-60. Skip when the page already carries an "arguments:" key.
# Anchors that must survive for the gate to splice the page (webfront/Program.cs:1369-1384):
#   * the literal token  "-AUTOSERVE"  must be present (fail-closed check) and the
#     splice replaces ", \"-AUTOSERVE\"" or "\"-AUTOSERVE\", " with the per-account
#     cluster, so it must stay a standalone array element.
#   * no literal  "-DATAID="  may appear in the template (identity must not be pinned).
#   * -FRONTURL is rewritten positionally by regex  -FRONTURL",\s*"[^"]*"  so it must
#     stay its own element followed by ", " and its own quoted value.
idx="$WEBROOT/index.html"                    # ps1 :43 (non-recursive -Filter index.html)
if [ ! -f "$idx" ]; then die "no index.html in $WEBROOT"; fi

MERGE_JS='(function(){var base=["-KONAMINET", "-FRONTURL", "__FRONTURL__", "-GAMELOG", "-NOIO", "-appBehavior", "NETCAFE", "-AUTOSERVE", "-TRANSLATE"];try{var q=location.search.substring(1).split("&");for(var i=0;i<q.length;i++){if(!q[i])continue;var kv=q[i].split("=");var k=decodeURIComponent(kv[0].replace(/\+/g," "));if(kv.length>1){base.push(k);base.push(decodeURIComponent(kv[1].replace(/\+/g," ")))}else{base.push(k)}}}catch(e){}return base})()'
MERGE_JS=${MERGE_JS//__FRONTURL__/$FRONT_URL}   # ps1 :50 (FrontUrl spliced in)

if ! has "$idx" 'arguments:'; then
  if has "$idx" 'var config = {'; then
    # ps1 :51-53: 'var config = { arguments: <merge>,' replaces 'var config = {'
    apply_patch "$idx" 'var config = {' "var config = { arguments: $MERGE_JS," all
  else
    # ps1 :55-56: loaders with no config literal get the args object as 2nd parameter
    apply_patch "$idx" 'createUnityInstance(canvas, ' "createUnityInstance(canvas, { arguments: $MERGE_JS }, " all
  fi
  printf 'patched index.html config.arguments\n'
else
  printf 'index.html already has arguments\n'
fi

# ---------------------------- 5. canvas + letterbox CSS ----------------------------
# ps1 :65-75. Unity's Default template pins 960x600 (1.6) while the CanvasScaler
# reference resolution is 1920x1080; the CSS makes the canvas fill the window as a
# 16:9 letterbox. ps1 :67 used -replace with a pattern that has no metacharacters, so
# the literal needle below matches the same text.
apply_patch "$idx" '<canvas id="unity-canvas" width=960 height=600 ' '<canvas id="unity-canvas" width=1280 height=720 ' all

CANVAS_CSS='<style>/*MFG-CANVAS*/html,body{margin:0;padding:0;overflow:hidden;background:#000}#unity-container{position:fixed!important;left:0!important;top:0!important;width:100vw!important;height:100vh!important;transform:none!important}#unity-canvas{position:fixed!important;left:50%!important;top:50%!important;transform:translate(-50%,-50%)!important;width:min(100vw,177.78vh)!important;height:min(56.25vw,100vh)!important;display:block!important}#unity-loading-bar,#unity-logo,#unity-fullscreen-button,#unity-progress-bar-full,#unity-webgl-logo,#unity-build-title{display:none!important}</style>'
if ! has "$idx" 'MFG-CANVAS'; then           # ps1 :73 idempotence marker
  apply_patch "$idx" '</head>' "${CANVAS_CSS}</head>" all
fi
printf 'patched index.html canvas->letterbox-fill\n'

# ---------------------------- 6. framework.js url -> argv ----------------------------
# ps1 :80-90. Single-point edit: replace only the FIRST 'var arguments_=[]' (the loader
# factory's frozen argv) with a query parser, tagged with the /*MFG-URLARGV*/ marker so
# the patch is idempotent and survives a Unity rebuild - a fresh framework.js has no
# marker, so the same single needle is found again on the next assemble. Everything else
# in the 516KB framework.js is left untouched, byte for byte.
FW="$WEBROOT/Build/webgl.framework.js"       # ps1 :80
if [ -f "$FW" ]; then
  if ! has "$FW" 'MFG-URLARGV'; then
    FW_INJECT='var arguments_=/*MFG-URLARGV*/(function(){var out=[];try{var s=location.search.substring(1).split("&");for(var i=0;i<s.length;i++){if(!s[i])continue;var kv=s[i].split("=");var k=decodeURIComponent(kv[0].replace(/\+/g," "));if(kv.length>1){out.push(k);out.push(decodeURIComponent(kv[1].replace(/\+/g," ")))}else{out.push(k)}}if(typeof Module!=="undefined"&&Module["arguments"]){out=Module["arguments"].concat(out)}}catch(e){}return out})()'
    apply_patch "$FW" 'var arguments_=[]' "$FW_INJECT" first
    printf 'patched framework.js url->argv\n'
  else
    printf 'framework.js already url->argv patched\n'
  fi
fi

# ---------------------------- 7. build stamp ----------------------------
# ps1 :96-99. Same input set, same order: the four Build artifacts that exist in the
# webroot (framework.js here is the ALREADY-patched copy, exactly as in the PowerShell
# run, because step 6 ran first). Same algorithm:
#   sha256(file bytes) -> UPPER-CASE hex, concatenated with no separator
#   -> sha256 of those ASCII bytes -> UPPER-CASE hex -> first 12 characters.
stamp_files="$WEBROOT/Build/webgl.loader.js $WEBROOT/Build/webgl.framework.js $WEBROOT/Build/webgl.wasm $WEBROOT/Build/webgl.data"
joined=''
for f in $stamp_files; do
  if [ -f "$f" ]; then joined="$joined$(hash_upper "$f")"; fi   # ps1 :96 Where-Object Test-Path
done
STAMP=$(printf '%s' "$joined" | _hash_stdin | tr '[:lower:]' '[:upper:]' | cut -c1-12)

# ps1 :101-104 literal '"/<asset>"' -> '"/<asset>?v=<stamp>"' (the gate ignores queries)
for asset in webgl.loader.js webgl.framework.js webgl.data webgl.wasm; do
  apply_patch "$idx" "\"/$asset\"" "\"/$asset?v=$STAMP\"" all
done

# ps1 :114 build_stamp.txt, UTF-8 no BOM, no trailing newline (Set-Content -NoNewline)
printf '%s' "$STAMP" > "$WEBROOT/build_stamp.txt"

# ---------------------------- 8. catalog stamp + refresh watchdog ----------------------------
# ps1 :105-131. Two <script> blocks injected before </head> so they run before the
# loader: (a) an XMLHttpRequest.open hook that appends ?v=<stamp> to every
# .../aa/catalog.json fetch (UnityWebRequest rides XHR; per-build cache key), and (b) a
# watchdog that polls /build_stamp.txt?n=<random> every 2 minutes (first hit after 20s)
# and shows a Reload prompt when the stamp changed.
WATCHDOG=$(cat <<'BLOCK'
<script>
(function(){var V="__STAMP__";var o=XMLHttpRequest.prototype.open;XMLHttpRequest.prototype.open=function(m,u){try{if(typeof u==="string"&&/\/aa\/catalog\.json(\?|$)/.test(u)){u+=(u.indexOf("?")>=0?"&":"?")+"v="+V;}}catch(e){}return o.apply(this,arguments);};})();
</script>
<script>
(function(){var STAMP="__STAMP__";function chk(){fetch("/build_stamp.txt?n="+Math.random(),{cache:"no-store"}).then(function(r){return r.ok?r.text():"";}).then(function(t){var s=(t||"").trim();if(s&&s!==STAMP)show();}).catch(function(){});}function show(){if(document.getElementById("mfg-update"))return;var d=document.createElement("div");d.id="mfg-update";d.style.cssText="position:fixed;left:50%;bottom:18px;transform:translateX(-50%);z-index:99999;background:#181820;color:#fff;padding:10px 14px;border-radius:8px;font:14px system-ui,sans-serif;box-shadow:0 4px 14px rgba(0,0,0,.45);display:flex;gap:10px;align-items:center";var m=document.createElement("span");m.textContent="New game version available";var b=document.createElement("button");b.textContent="Reload";b.style.cssText="background:#4a6cf7;color:#fff;border:0;border-radius:6px;padding:6px 12px;cursor:pointer";b.onclick=function(){location.reload();};d.appendChild(m);d.appendChild(b);document.body.appendChild(d);}setTimeout(chk,20000);setInterval(chk,120000);})();
</script>
BLOCK
)
WATCHDOG=${WATCHDOG//__STAMP__/$STAMP}       # ps1 :123-124 (two Replace calls; all
#                                            occurrences each - one pass here is the same)
if has "$idx" '</head>'; then
  apply_patch "$idx" '</head>' "${WATCHDOG}</head>" all
else
  # ps1 :127-129 no </head> to splice into: the block is appended at the end
  printf '%s' "$WATCHDOG" >> "$idx"
fi
printf 'stamped build asset urls v=%s; catalog-fetch stamp + auto-refresh watchdog baked\n' "$STAMP"

# ---------------------------- 9. brotli sidecars ----------------------------
# ps1 :133-135 called scripts\brotli-sidecar\bin\Release\net8.0\sidecar.exe <webroot>.
# The tool is net8.0 and framework-dependent (Program.cs), so on Linux run its assembly
# through the runtime: dotnet <SIDECAR_DLL> <webroot> <quality>. Same tool means the same
# skip rules (png/jpg/webm/ogg/mp3 magic bytes, files under 1024 B, *.br itself) and the
# same quality 6 default (Program.cs:15,22-28); wasm is NOT skipped.
# If a web server front-ends this webroot later, nginx brotli_static on; can serve these
# same .br sidecars instead of the gate, no re-encoding needed.
if [ "$NO_BROTLI" = 1 ]; then
  printf 'skipped brotli sidecars (--no-brotli)\n'
else
  if [ ! -f "$SIDECAR_DLL" ]; then die "no sidecar tool at $SIDECAR_DLL"; fi
  if ! command -v dotnet >/dev/null 2>&1; then die "dotnet runtime not on PATH (needed for $SIDECAR_DLL)"; fi
  dotnet "$SIDECAR_DLL" "$WEBROOT" "$BROTLI_QUALITY"
fi

# ---------------------------- 10. optional serve ----------------------------
# ps1 :136-147. Off by default; nothing is started unless SERVE=1 / --serve.
# WEBFRONT_GATE=0 is relay mode, as in the PowerShell dev serve. The pid this script
# starts is recorded in GATE_PID_FILE - the same single gate pid file lane_up.sh writes
# and lane_down.sh stops - so the next assemble run can stop it. It is never recorded
# over a live pid: step 2 already stopped the recorded gate (or --no-gate-stop was asked
# for, in which case the record belongs to somebody else and is left alone).
if [ "$SERVE" = 1 ]; then
  if [ ! -f "$WEBFRONT_DLL" ]; then die "no webfront build at $WEBFRONT_DLL"; fi
  if ! command -v dotnet >/dev/null 2>&1; then die "dotnet runtime not on PATH (needed for $WEBFRONT_DLL)"; fi
  if [ -f "$GATE_PID_FILE" ]; then
    _live=$(sed -n '1p' "$GATE_PID_FILE" 2>/dev/null | tr -d '\r\t' | sed 's/^ *//; s/ *$//')
    case $_live in
      ''|*[!0-9]*) : ;;
      *) if pid_alive "$_live"; then
           die "--serve would overwrite the gate record $GATE_PID_FILE while pid $_live is live: stop that gate first (lane_down.sh) or assemble without --serve"
         fi ;;
    esac
  fi
  WEBFRONT_ROOT=$WEBROOT
  WEBFRONT_UPSTREAM=$UPSTREAM
  WEBFRONT_BIND=$BIND
  WEBFRONT_GATE=0
  export WEBFRONT_ROOT WEBFRONT_UPSTREAM WEBFRONT_BIND WEBFRONT_GATE
  _logdir=${TMPDIR:-/tmp}
  ( cd "${WEBFRONT_DLL%/*}" && exec dotnet "${WEBFRONT_DLL##*/}" ) \
    >"$_logdir/webfront.out.txt" 2>"$_logdir/webfront.err.txt" &
  servePid=$!
  mkdir -p -- "$PID_DIR"
  echo "$servePid" > "$GATE_PID_FILE"
  printf 'webfront started (pid %s, recorded in %s) bind=%s WEBFRONT_UPSTREAM=%s\n' "$servePid" "$GATE_PID_FILE" "$BIND" "$UPSTREAM"
fi
